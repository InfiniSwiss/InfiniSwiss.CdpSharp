using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using InfiniSwiss.CdpSharp.Exceptions;
using Newtonsoft.Json;
using Websocket.Client;

namespace InfiniSwiss.CdpSharp.WebSocketCommunication
{
    public class CdpCommunicator : IDisposable
    {
        public async Task InitializeAsync(string url, int remoteDebuggingPort, string initialUrl)
        {
            // We open a new tab(in case the chromium is shared between multiple consumers, we'll have our own tab where we can navigate.
            
            var cdpNewUrl = $"http://{url}:{remoteDebuggingPort}/json/new?{initialUrl}";
            var cdpResultJson = await new HttpClient().GetStringAsync(cdpNewUrl);
            var cdpResult = JsonConvert.DeserializeAnonymousType(cdpResultJson, new { webSocketDebuggerUrl = "", id ="" });

            var webSocketDebuggerUrl = cdpResult?.webSocketDebuggerUrl;
            this.targetId = cdpResult.id;
            this.url = url;
            this.remoteDebuggingPort = remoteDebuggingPort;

            if (string.IsNullOrEmpty(webSocketDebuggerUrl))
            {
                throw new InvalidOperationException($"Could not read the chromium websocket debugger url from {cdpNewUrl}");
            }

            this.webSocket = new WebsocketClient(new Uri(webSocketDebuggerUrl));
            this.webSocket.MessageReceived.Subscribe(this.HandleWebSocketMessage);

            await this.webSocket.Start();
        }

        public Task<string> SendAsync(string method, object parameters = null)
        {
            var requestMessage = $@"
            {{
                ""id"": {this.requestId},
                ""method"": ""{method}"",
                ""params"": {JsonConvert.SerializeObject(parameters)}
            }}";

            var tcs = new TaskCompletionSource<string>();
            this.pendingCommands.Add(this.requestId, tcs);

            this.webSocket.Send(requestMessage);

            this.requestId += 1;

            return tcs.Task;
        }

        public void RegisterEventHandler(string eventName, Action handler)
        {
            if (!this.eventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers = new List<Action>();
                this.eventHandlers.Add(eventName, handlers);
            }

            handlers.Add(handler);
        }

        public void UnregisterEventHandler(string eventName, Action handler)
        {
            if (!this.eventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers = new List<Action>();
                this.eventHandlers.Add(eventName, handlers);
            }

            handlers.Remove(handler);
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                var cdpCloseUrl = $"http://{url}:{remoteDebuggingPort}/json/close/{targetId}";
                // we need also to close our tab
                this.webSocket.Dispose();
                var httpResult = new HttpClient().GetAsync(cdpCloseUrl).Result;
                if (httpResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new CloseChromiumTabException($"Cannot close chromium tab using url: {cdpCloseUrl}");
                }
            }

            disposed = true;
        }

        private void HandleWebSocketMessage(ResponseMessage message)
        {
            if (message.MessageType != WebSocketMessageType.Text)
            {
                return;
            }

            if (message.Text.Contains("error"))
            {
                var errorResponse = JsonConvert.DeserializeAnonymousType(message.Text, new { error = new { code = string.Empty, message = string.Empty } });
                throw new CdpCommunicationException(errorResponse.error.code, errorResponse.error.message);
            }

            if (message.Text.Contains("method"))
            {
                var eventResponse = JsonConvert.DeserializeAnonymousType(message.Text, new { Method = string.Empty });
                this.NotifyEventHandlers(eventResponse.Method);
                return;
            }

            var commandResponse = JsonConvert.DeserializeAnonymousType(message.Text, new { Id = 1 });
            if (this.pendingCommands.TryGetValue(commandResponse.Id, out var tcs))
            {
                tcs.SetResult(message.Text);
                this.pendingCommands.Remove(commandResponse.Id);
            }
        }

        private void NotifyEventHandlers(string eventName)
        {
            if (!this.eventHandlers.TryGetValue(eventName, out var handlers))
            {
                return;
            }

            foreach (var handler in handlers.ToList())
            {
                handler();
            }
        }

        private int requestId = 1;
        private readonly Dictionary<int, TaskCompletionSource<string>> pendingCommands = new Dictionary<int, TaskCompletionSource<string>>();
        private readonly Dictionary<string, List<Action>> eventHandlers = new Dictionary<string, List<Action>>();
        private WebsocketClient webSocket;
        private bool disposed;
        private string targetId;
        private string url;
        private int remoteDebuggingPort;
    }
}
