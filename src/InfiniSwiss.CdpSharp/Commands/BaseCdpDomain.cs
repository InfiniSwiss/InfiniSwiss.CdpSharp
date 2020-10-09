using InfiniSwiss.CdpSharp.WebSocketCommunication;

namespace InfiniSwiss.CdpSharp.Commands
{
    public abstract class BaseCdpDomain
    {
        protected BaseCdpDomain(CdpCommunicator cdpCommunicator)
        {
            Communicator = cdpCommunicator;
        }

        public CdpCommunicator Communicator { get; }
    }
}
