
# InfiniSwiss.CdpSharp
A library to easily use the Chrome DevTools Protocol https://chromedevtools.github.io/devtools-protocol/

## Getting started
To install InfiniSwiss.CdpSharp, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

    PM> Install-Package InfiniSwiss.CdpSharp

## Chrome DevTools Protocol
The Chrome DevTools Protocol (CDP) provides a way to interact with Chromium-based browser to control, instrument, inspect, debug and profile them.

The protocol's API is divided into a number of domains (Page, Dom, Network, Console etc). To use the protocol you have to:

 1. Start the Chromium browser (either normally or headless)
 2. Connect to it using websockets
 3. Send commands
 4. Receive events

Please read the [CDP docs](https://chromedevtools.github.io/devtools-protocol/) for more information on the possible domains, commands and events.

## Usage
This library provides a base for using the CDP protocol. It provides the following features:

 1. Control the lifecycle of the Chromium browser instance
 2. Abstract away the websocket connection
 3. Provide a simple interface to send commands and receive events

Please take a look at the demo app for a simple usage scenario. The app:

 1. Starts a Chromium browser instance
 2. Navigates it to a page
 3. Prints the page to a PDF file on disk.
 
 ```
  var sourceUrl = "https://developer.mozilla.org/en-US/docs/Web/CSS/@media";
  var pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "out.pdf");

// The context manages the lifetime of the Chromium browser instance. When started it will start the browser (doing a best effort search for the executable if not specified) and connect to its CDP via websocket. When disposed it will disconnect from the websocket and kill the browser instance.
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
 ```

## License
MIT
[https://licenses.nuget.org/MIT](https://licenses.nuget.org/MIT)
