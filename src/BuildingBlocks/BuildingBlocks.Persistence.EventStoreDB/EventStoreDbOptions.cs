namespace BuildingBlocks.Persistence.EventStoreDB;

public class EventStoreDbOptions
{
    public bool UseInternalCheckpointing { get; set; } = true;
    public string ConnectionString { get; set; } = default!;
    public EventStoreDbSubscriptionOptions SubscriptionOptions { get; set; } = null!;
}

public class EventStoreDbSubscriptionOptions
{
    public string SubscriptionId { get; set; } = "default";
    public bool ResolveLinkTos { get; set; }
    public bool IgnoreDeserializationErrors { get; set; } = true;
}
