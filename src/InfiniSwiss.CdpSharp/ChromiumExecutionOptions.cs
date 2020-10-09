
namespace InfiniSwiss.CdpSharp
{
    public class ChromiumExecutionOptions
    {
        public string ChromiumPath { get; set; }

        public int RemoteDebuggingPort { get; set; } = 9222;

        public bool RunHeadless { get; set; }
    }
}
