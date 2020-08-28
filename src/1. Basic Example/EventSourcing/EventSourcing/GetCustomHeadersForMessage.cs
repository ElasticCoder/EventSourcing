namespace EventSourcing
{
    using System.Collections.Generic;

    public delegate IDictionary<string, object> GetCustomHeadersForMessage(object body, Aggregate aggregate);
}
