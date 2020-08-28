using System;

namespace EventSourcing
{
    public class StreamConcurrencyException : ApplicationException
    {
        public StreamConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
