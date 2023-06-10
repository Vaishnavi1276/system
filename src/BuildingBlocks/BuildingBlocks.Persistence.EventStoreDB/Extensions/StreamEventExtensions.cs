using BuildingBlocks.Core.Persistence.EventStore;
using EventStore.Client;

namespace BuildingBlocks.Persistence.EventStoreDB.Extensions;

public static class StreamEventExtensions
{
    public static IEnumerable<StreamEvent> ToStreamEvents(this IEnumerable<ResolvedEvent> resolvedEvents)
    {
        return resolvedEvents.Select(x => x.ToStreamEvent());
    }
}
