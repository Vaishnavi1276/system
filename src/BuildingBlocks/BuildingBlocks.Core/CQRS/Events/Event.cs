using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Core.Types;

namespace BuildingBlocks.Core.CQRS.Events;

public record Event : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public long EventVersion => -1;
    public DateTime OccurredOn { get; } = DateTime.Now;
    public DateTimeOffset TimeStamp { get; } = DateTimeOffset.Now;
    public string EventType => TypeMapper.GetFullTypeName(GetType());
}
