using InfiniSwiss.CdpSharp.Commands.Page;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InfiniSwiss.CdpSharp.WebSocketCommunication;

namespace InfiniSwiss.CdpSharp
{
    public class ChromiumExecutionContext : IDisposable
    {
        public ChromiumExecutionContext(ChromiumExecutionOptions options = null)
        {
            this.options = options ?? new ChromiumExecutionOptions();
        }

        ~ChromiumExecutionContext() => Dispose(false);

        public CdpPageDomain Page { get; private set; }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                this.cdpCommunicator.Dispose();
            }

            this.chromiumProcess?.Kill();

            disposed = true;
        }

        public async Task StartAsync()
        {
            var chromiumPath = this.GetChromiumPath(options.ChromiumPath);
            if (string.IsNullOrEmpty(chromiumPath) || !File.Exists(chromiumPath))
            {
                throw new FileNotFoundException("Could not find either Edge or Chrome executable");
            }

            this.chromiumProcess = Process.Start(chromiumPath, $"{(this.options.RunHeadless ? "--headless" : string.Empty)} --remote-debugging-port={options.RemoteDebuggingPort}");

            this.cdpCommunicator = new CdpCommunicator();
            await this.cdpCommunicator.InitializeAsync(this.options.RemoteDebuggingPort);

            this.Page = new CdpPageDomain(this.cdpCommunicator);
        }

        private string GetChromiumPath(string preferredPath)
        {
            if (!string.IsNullOrEmpty(preferredPath) && File.Exists(preferredPath))
            {
                return preferredPath;
            }

            var knownChromiumFilePath = knownChromiumPaths.FirstOrDefault(File.Exists);
            if (knownChromiumFilePath != null)
            {
                return knownChromiumFilePath;
            }

            return Directory.EnumerateFiles("C:\\Program Files (x86)", "*.exe", SearchOption.AllDirectories)
                .FirstOrDefault(path => path.Contains("msedge.exe") || path.Contains("chrome.exe"));
        }

        private readonly ChromiumExecutionOptions options;
        private bool disposed;
        private Process chromiumProcess;
        private CdpCommunicator cdpCommunicator;
        private readonly string[] knownChromiumPaths = 
        {
            "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
            "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"
        };
    }
}
