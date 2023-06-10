using BuildingBlocks.Abstractions.Domain.Events;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.Integration.External;

public class SupplierUpdatedConsumer
    : IEventHandler<Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierUpdatedV1>
{
    public Task Handle(
        Services.Shared.Catalogs.Suppliers.Events.v1.Integration.SupplierUpdatedV1 notification,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }
}
