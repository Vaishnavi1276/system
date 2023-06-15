using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Abstractions.Domain.Events;

public interface IInternalEventBus
{
    Task Publish(IEvent @event, CancellationToken ct);
    Task Publish(IMessage @event, CancellationToken ct);
}
