
namespace InfiniSwiss.CdpSharp
{
    public class ChromiumExecutionOptions
    {
        public string ChromiumPath { get; set; }

        public int RemoteDebuggingPort { get; set; } = 9222;

        public string RemoteChromiumUrl { get; set; }

        public bool RunHeadless { get; set; }

        public string InitialUrl = "about:blank";
    }
}
