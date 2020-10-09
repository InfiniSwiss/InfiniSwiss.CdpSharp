using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Websocket.Client;

namespace InfiniSwiss.CdpSharp.WebSocketCommunication
{
    public class CdpCommunicator : IDisposable
    {
        public async Task InitializeAsync(int remoteDebuggingPort)
        {
            var cdpListUrl = $"http://localhost:{remoteDebuggingPort}/json/list";
            var cdpListResultJson = await new HttpClient().GetStringAsync(cdpListUrl);
            var cdpListResult = JsonConvert.DeserializeAnonymousType(cdpListResultJson, new[] { new { webSocketDebuggerUrl = "" } });

            var webSocketDebuggerUrl = cdpListResult?.FirstOrDefault()?.webSocketDebuggerUrl;
            if (string.IsNullOrEmpty(webSocketDebuggerUrl))
            {
                throw new InvalidOperationException($"Could not read the chromium websocket debugger url from {cdpListUrl}");
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
                this.webSocket.Dispose();
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
    }
}
