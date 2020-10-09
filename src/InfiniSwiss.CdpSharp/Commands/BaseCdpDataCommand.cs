using InfiniSwiss.CdpSharp.WebSocketCommunication;
using Newtonsoft.Json;

namespace InfiniSwiss.CdpSharp.Commands
{
    public abstract class BaseCdpDataCommand : BaseCdpCommand
    {
        protected BaseCdpDataCommand(CdpCommunicator communicator) : base(communicator)
        {
        }

        protected string GetDataFromCdpResponseMessage(string responseMessageString)
        {
            var responseMessage = JsonConvert.DeserializeAnonymousType(responseMessageString, new
            {
                Id = 1,
                Result = new
                {
                    Data = string.Empty
                }
            });

            return responseMessage.Result?.Data;
        }
    }
}
