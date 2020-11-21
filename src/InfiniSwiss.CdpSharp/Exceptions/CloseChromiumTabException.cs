using System;
using System.Runtime.Serialization;

namespace InfiniSwiss.CdpSharp.Exceptions
{
    public class CloseChromiumTabException : Exception
    {
        public CloseChromiumTabException()
        {
        }

        public CloseChromiumTabException(string message) : base(message)
        {
        }

        public CloseChromiumTabException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CloseChromiumTabException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
