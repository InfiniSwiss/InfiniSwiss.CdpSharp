using System;

namespace InfiniSwiss.CdpSharp.WebSocketCommunication
{
    public class CdpCommunicationException : Exception
    {
        public CdpCommunicationException(string errorCode, string errorMessage)
        {
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
        }

        public string ErrorCode { get; }
        
        public string ErrorMessage { get; }
    }
}
