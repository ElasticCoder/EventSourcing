# Event Sourcing Tutorial


## Simple Event Store
Simple Event Store is an Event Sourcing framework that is available in [GitHub](https://github.com/ASOS/SimpleEventStore).
### Class diagram
![Class Diagram](./ClassDiagram.png)

Aggregates inherit from the *Aggregate* class.

The *DomainRepository* class allows you to load an save aggregates from an *EventStore*. 

The EventStore is associated with a *StorageEngine*, allowing you to choose from CosmosDB, InMemory, or any other data store that you prefer.

## Conflict Resolution
When an aggregate is loaded the event stream is held in memory and any new updates are appended to the end of the stream. If two instances of the same aggregate (having the same identity) are modified in parallel then when the second instance is saved a StreamConcurrencyException is thrown. If the business process is likely to perform a lot of parallel operations on the same stream then these concurrency errors can degrade performance.

[This article](https://www.michielrook.nl/2016/09/concurrent-commands-event-sourcing/#:~:text=Conflict%20resolution) describes conflict resolution. It is possible and may be desirable to enhance the SimpleEventStore framework to support conflict resolution. This would allow certain, whitelisted, events to be written concurrently. When the writer detects that the stream has been updated and the stream version number has been modified, it reloads the stream and then reattempts to append to it.

