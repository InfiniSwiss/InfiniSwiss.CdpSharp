
using System;
using System.Threading.Tasks;
using InfiniSwiss.CdpSharp.WebSocketCommunication;

namespace InfiniSwiss.CdpSharp.Commands.Page.Navigation
{
    public class CdpPageNavigationCommand : BaseCdpCommand
    {
        public CdpPageNavigationCommand(CdpCommunicator communicator) : base(communicator)
        {
        }

        public async Task ExecuteAsync(string url)
        {
            await this.ExecuteAsync(new CdpPageNavigationCommandOptions() { Url = url });
        }

        public async Task<bool> ExecuteAsync(CdpPageNavigationCommandOptions options)
        {
            try
            {
                navigationCompleteTcs = new TaskCompletionSource<bool>();

                Communicator.RegisterEventHandler("Page.loadEventFired", FrameNavigatedEventHandler);

                await Communicator.SendAsync("Page.navigate", new
                {
                    url = options.Url
                });

                return await navigationCompleteTcs.Task;
            }
            catch
            {
                Communicator.UnregisterEventHandler("Page.loadEventFired", FrameNavigatedEventHandler);

                throw;
            }
        }

        private void FrameNavigatedEventHandler()
        {
            Communicator.UnregisterEventHandler("Page.loadEventFired", FrameNavigatedEventHandler);
            this.navigationCompleteTcs.SetResult(true);
        }

        private TaskCompletionSource<bool> navigationCompleteTcs;
    }
}
