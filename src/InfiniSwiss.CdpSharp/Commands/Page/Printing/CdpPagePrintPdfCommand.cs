using System.Threading.Tasks;
using InfiniSwiss.CdpSharp.WebSocketCommunication;

namespace InfiniSwiss.CdpSharp.Commands.Page.Printing
{
    public class CdpPagePrintPdfCommand : BaseCdpDataCommand
    {
        public CdpPagePrintPdfCommand(CdpCommunicator communicator) : base(communicator)
        {
        }

        public bool DisplayHeaderFooter { get; set; }

        public string HeaderTemplate { get; set; }
        
        public string FooterTemplate { get; set; }

        public const string DefaultFooterTemplate = @"<div style='width: 100%; text-align: right; margin-left: 0.4in; margin-right: 0.4in'>
                                                         <span style='font-size: 8px' class='pageNumber'></span> 
                                                         <span style='font-size: 8px'>&nbsp;/&nbsp;</span> 
                                                         <span style='font-size: 8px' class='totalPages'></span>
                                                      </div>";

        public async Task<string> ExecuteAsync()
        {
            var responseMessage = await Communicator.SendAsync("Page.printToPDF", new 
            {
                displayHeaderFooter = DisplayHeaderFooter,
                headerTemplate = HeaderTemplate ?? "<div></div>",
                footerTemplate = FooterTemplate ?? "<div></div>"
            });

            return base.GetDataFromCdpResponseMessage(responseMessage);
        }
    }
}
