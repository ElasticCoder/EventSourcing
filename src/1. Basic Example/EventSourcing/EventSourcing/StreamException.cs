using System;

namespace EventSourcing
{
    public class StreamException : ApplicationException
    {
        public StreamException(string message) : base(message)
        {
        }
    }
}
