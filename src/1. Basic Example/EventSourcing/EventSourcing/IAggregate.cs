namespace EventSourcing
{
    using System.Collections.Generic;

    public interface IAggregate
    {
        string Id { get; }

        void ApplyEvents(IEnumerable<EventStoreMessage> eventStream);
    }
}