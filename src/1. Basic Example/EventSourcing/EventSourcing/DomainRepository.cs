namespace EventSourcing
{
    using SimpleEventStore;
    using SimpleEventStore.InMemory;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///  An abstraction over the event store.
    /// </summary>
    public class DomainRepository
    {
        private EventStore eventStore;
        private GetCustomHeadersForMessage NoHeaders = (b, a) => new Dictionary<string, object>();
        private const string CausationId = "CausationId";

        /// <summary>
        ///  Creates a domain repository.
        /// </summary>
        /// <param name="eventStore">The underlying event store.</param>
        public DomainRepository(EventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        /// <summary>
        ///  Loads and applies events to the aggregate.
        /// </summary>
        /// <param name="aggregate">The aggregate in which the events are applied.</param>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>A Task</returns>
        public async Task LoadAsync(Aggregate aggregate, CancellationToken cancellation = default)
        {
            var stream = await eventStore.ReadStreamForwards(aggregate.Id, cancellation);

            if (stream.Any())
            {
                aggregate.ApplyEvents(MapEvents(stream));
            }
        }

        public async Task SaveAsync(Aggregate aggregate)
        {
            await SaveAsync(aggregate, CausationId, NoHeaders);
            
        }

        public async Task SaveAsync(Aggregate aggregate, string causationId, GetCustomHeadersForMessage getHeaders)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            if (! aggregate.UncommittedEvents.Any())
            {
                return;
            }

            var events = new List<EventData>();

            foreach (var uncommittedEvent in aggregate.UncommittedEvents)
            {
                var headers = new Dictionary<string, object>
                {
                    { CausationId, causationId }
                };
                foreach (var h in getHeaders(uncommittedEvent, aggregate))
                {
                    headers.Add(h.Key, h.Value);
                }

                events.Add(new EventData(Guid.NewGuid(), uncommittedEvent, headers));
            }

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
    }
}
