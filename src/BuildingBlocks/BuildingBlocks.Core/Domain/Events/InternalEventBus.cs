using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Messaging;
using MediatR;
using Polly;

namespace BuildingBlocks.Core.Domain.Events;

public class InternalEventBus : IInternalEventBus
{
    private readonly IMediator _mediator;

    public InternalEventBus(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Publish(IEvent @event, CancellationToken ct)
    {
        var policy = Policy.Handle<System.Exception>().RetryAsync(2);

        await policy.ExecuteAsync(c => _mediator.Publish(@event, c), ct);
    }

    public async Task Publish(IMessage @event, CancellationToken ct)
    {
        var policy = Policy.Handle<System.Exception>().RetryAsync(2);

        await policy.ExecuteAsync(c => _mediator.Publish(@event, c), ct);
    }
}
