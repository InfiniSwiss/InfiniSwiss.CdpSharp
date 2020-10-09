using System.Threading.Tasks;
using InfiniSwiss.CdpSharp.WebSocketCommunication;

namespace InfiniSwiss.CdpSharp.Commands.Page.Printing
{
    public class CdpPagePrintPdfCommand : BaseCdpDataCommand
    {
        public CdpPagePrintPdfCommand(CdpCommunicator communicator) : base(communicator)
        {
        }

        public async Task<string> ExecuteAsync()
        {
            var responseMessage = await Communicator.SendAsync("Page.printToPDF");
            return base.GetDataFromCdpResponseMessage(responseMessage);
        }
    }
}
