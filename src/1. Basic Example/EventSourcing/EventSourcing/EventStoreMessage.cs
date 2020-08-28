namespace EventSourcing
{
    using System;
    using System.Collections.Generic;

    public class EventStoreMessage
    {
        public EventStoreMessage(Guid eventId, object body)
        {
            this.EventId = eventId;
            this.Body = body;
            this.Headers = new Dictionary<string, object>();
        }

        public Guid EventId { get; private set; }

        public object Body { get; private set; }

        public Dictionary<string, object> Headers { get; private set; }
    }
}
