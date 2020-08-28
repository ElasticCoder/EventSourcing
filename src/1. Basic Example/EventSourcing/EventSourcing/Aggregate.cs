namespace EventSourcing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class Aggregate : IAggregate
    {
        private static readonly ConcurrentDictionary<Tuple<Type, Type>, MethodInfo> UpdateStateMethodCache = new ConcurrentDictionary<Tuple<Type, Type>, MethodInfo>();

        private const string DefaultAggregateUpdateStateMethodName = "UpdateState";

        protected virtual string UpdateStateMethodName { get; set; }

        public string Id { get; private set; }

        public int EventStreamRevision { get; set; }

        public IList<object> UncommittedEvents { get; private set; }

        protected Aggregate(string id)
        {
            this.Id = id;
            this.UncommittedEvents = new List<object>();
        }

        public void ApplyEvents(IEnumerable<EventStoreMessage> eventStream)
        {
            if (eventStream == null)
            {
                throw new ArgumentNullException(nameof(eventStream));
            }

            foreach (EventStoreMessage eventStoreMessage in eventStream)
            {
                this.ApplyObject(eventStoreMessage.Body);
                this.EventStreamRevision++;
            }
        }

        protected void Apply(params object[] domainEvents)
        {
            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            foreach (object domainEvent in domainEvents)
            {
                this.ApplyObject(domainEvent);
                this.UncommittedEvents.Add(domainEvent);
            }
        }

        private void ApplyObject(object eventMessage)
        {
            string updateStateMethodNameToUse = string.IsNullOrEmpty(this.UpdateStateMethodName) ? DefaultAggregateUpdateStateMethodName : this.UpdateStateMethodName;
            object message = eventMessage;

            // Key off a combination of the aggregate type and the message type (multiple aggregates may accept the same message).
            Tuple<Type, Type> updateStateMethodCacheKey = new Tuple<Type, Type>(this.GetType(), eventMessage.GetType());
            MethodInfo updateStateMethodInfo = UpdateStateMethodCache.GetOrAdd(
                updateStateMethodCacheKey,
                mi => this.GetType().GetAggregateUpdateStateMethodForMessage(updateStateMethodNameToUse, message.GetType()));
            if (updateStateMethodInfo != null)
            {
                updateStateMethodInfo.Invoke(this, new[] { eventMessage });
            }
        }
    }
}
