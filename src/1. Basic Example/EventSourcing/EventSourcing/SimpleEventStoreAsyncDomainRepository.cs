namespace EventSourcing
{
    using SimpleEventStore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SimpleEventStoreAsyncDomainRepository
    {
        private EventStore eventStore;
        private const string CausationId = "CausationId";

        private GetCustomHeadersForMessage NoHeaders = (b, a) => new Dictionary<string, object>();

        public SimpleEventStoreAsyncDomainRepository(EventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task<bool> ExistsAsync(string eventStreamId)
        {
            return (await eventStore.ReadStreamForwards(eventStreamId, 0, 1).ConfigureAwait(false)).Any();
        }

        public async Task<bool> ExistsAsync(Aggregate aggregate)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));

            return await this.ExistsAsync(aggregate.Id).ConfigureAwait(false);
        }

        // REVIEW: Should we be supporting this?  Feels odd to throw an exception
        public async Task LoadAsync(Aggregate aggregate)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));

            var stream = await eventStore.ReadStreamForwards(aggregate.Id).ConfigureAwait(false);

            if (!stream.Any()) throw new StreamException($"Event stream {aggregate.Id} does not exist");

            aggregate.ApplyEvents(MapEvents(stream));
        }

        private static IEnumerable<EventStoreMessage> MapEvents(IReadOnlyCollection<StorageEvent> stream)
        {
            return stream.Select(e =>
            {
                var message = new EventStoreMessage(e.EventId, e.EventBody);
                if (e.Metadata != null)
                {
                    foreach (var h in (Dictionary<string, object>)e.Metadata)
                    {
                        message.Headers.Add(h.Key, h.Value);
                    }
                }

                return message;
            });
        }

        public async Task<bool> LoadIfExistsAsync(Aggregate aggregate)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));

            var stream = await eventStore.ReadStreamForwards(aggregate.Id).ConfigureAwait(false);
            aggregate.ApplyEvents(MapEvents(stream));

            return stream.Any();
        }

        public Task SaveAsync(Aggregate aggregate)
        {
            return this.SaveAsync(aggregate, string.Empty, NoHeaders);
        }

        public Task SaveAsync(Aggregate aggregate, string causationId)
        {
            return this.SaveAsync(aggregate, causationId, NoHeaders);
        }

        public async Task SaveAsync(Aggregate aggregate, string causationId, GetCustomHeadersForMessage getHeaders)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));

            var events = new List<EventData>();

            foreach (var uncommittedEvent in aggregate.UncommittedEvents)
            {
                var headers = new Dictionary<string, object>();
                headers.Add(CausationId, causationId);
                foreach (var h in getHeaders(uncommittedEvent, aggregate))
                {
                    headers.Add(h.Key, h.Value);
                }

                events.Add(new EventData(Guid.NewGuid(), uncommittedEvent, headers));
            }

            if (!events.Any()) throw new StreamException($"Aggregate {aggregate.Id} needs uncommitted events to save.");

            try
            {
                await eventStore.AppendToStream(aggregate.Id, aggregate.EventStreamRevision, events.ToArray()).ConfigureAwait(false);
                aggregate.UncommittedEvents.Clear();
                aggregate.EventStreamRevision += events.Count;
            }
            catch (ConcurrencyException e)
            {
                throw new StreamConcurrencyException(e.Message, e);
            }
        }
    }
}
