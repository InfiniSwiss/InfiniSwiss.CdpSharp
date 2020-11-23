using System;
using System.IO;
using System.Threading.Tasks;
using InfiniSwiss.CdpSharp;

namespace Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await RunWithExistingChromium();
            await RunWithRemoteChromium();
        }

        static async Task RunWithExistingChromium()
        {
            var sourceUrl = "https://developer.mozilla.org/en-US/docs/Web/CSS/@media";
            var pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "out.pdf");

            // The context manages the lifetime of the Chromium browser instance. When started it will start the browser (doing
            // a best effort search for the executable if not specified) and connect to its CDP via websocket. When disposed it
            // will disconnect from the websocket and kill the browser instance.
            using var chromiumExecutionContext = new ChromiumExecutionContext(new ChromiumExecutionOptions() { RunHeadless = true });
            await chromiumExecutionContext.StartAsync();

            // Navigate to a url. The command returns no result but will wait for the navigation to complete.
            var pageNavigationCommand = chromiumExecutionContext.Page.CreateNavigationCommand();
            await pageNavigationCommand.ExecuteAsync(sourceUrl);

            // Now that the navigation is complete print the page. The command returns the printed page as a base64 string.
            var pagePrintToPdfCommand = chromiumExecutionContext.Page.CreatePrintPdfCommand();
            var base64Pdf = await pagePrintToPdfCommand.ExecuteAsync();

            // Save the file to disk
            File.WriteAllBytes(pdfFilePath, Convert.FromBase64String(base64Pdf));
        }

        static async Task RunWithRemoteChromium()
        {
            var sourceUrl = "https://developer.mozilla.org/en-US/docs/Web/CSS/@media";
            var pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "out.pdf");

            // The context manages the lifetime of the remote Chromium browser tab. When started it will start a new tab and connect to its CDP via websocket. When disposed it
            // will disconnect from the websocket and close the tab.
            using var chromiumExecutionContext = new ChromiumExecutionContext(new ChromiumExecutionOptions() { RunHeadless = true, RemoteChromiumUrl = "localhost", InitialUrl = sourceUrl });
            await chromiumExecutionContext.StartAsync();

            // Now that the navigation is complete print the page. The command returns the printed page as a base64 string.
            var pagePrintToPdfCommand = chromiumExecutionContext.Page.CreatePrintPdfCommand();
            var base64Pdf = await pagePrintToPdfCommand.ExecuteAsync();

            // Save the file to disk
            File.WriteAllBytes(pdfFilePath, Convert.FromBase64String(base64Pdf));
        }

    }
}
