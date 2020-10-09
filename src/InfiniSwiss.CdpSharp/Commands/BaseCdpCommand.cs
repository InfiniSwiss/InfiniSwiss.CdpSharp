using InfiniSwiss.CdpSharp.WebSocketCommunication;

namespace InfiniSwiss.CdpSharp.Commands
{
    public abstract class BaseCdpCommand
    {
        protected BaseCdpCommand(CdpCommunicator communicator)
        {
            this.Communicator = communicator;
        }

        protected CdpCommunicator Communicator { get; }
    }
}
