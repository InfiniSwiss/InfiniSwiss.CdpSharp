using InfiniSwiss.CdpSharp.Commands.Page.Navigation;
using InfiniSwiss.CdpSharp.Commands.Page.Printing;
using InfiniSwiss.CdpSharp.WebSocketCommunication;

namespace InfiniSwiss.CdpSharp.Commands.Page
{
    public class CdpPageDomain : BaseCdpDomain
    {
        public CdpPageDomain(CdpCommunicator cdpCommunicator) : base(cdpCommunicator)
        {
            Communicator.SendAsync("Page.enable");
        }

        public CdpPageNavigationCommand CreateNavigationCommand()
        {
            return new CdpPageNavigationCommand(Communicator);
        }

        public CdpPagePrintPdfCommand CreatePrintPdfCommand()
        {
            return new CdpPagePrintPdfCommand(Communicator);
        }
    }
}
