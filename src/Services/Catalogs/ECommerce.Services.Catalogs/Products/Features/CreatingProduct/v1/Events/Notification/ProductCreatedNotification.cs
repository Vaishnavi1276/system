using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Domain;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Notification;

internal record ProductCreatedNotification(ProductCreated DomainEvent)
    : BuildingBlocks.Core.CQRS.Events.Internal.DomainNotificationEventWrapper<ProductCreated>(DomainEvent);

internal class ProductCreatedHandler : IDomainNotificationEventHandler<ProductCreatedNotification>
{
    private readonly IBus _bus;

    public ProductCreatedHandler(IBus bus)
    {
        _bus = bus;
    }

    public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        // We could publish integration event to bus here
        // await _bus.PublishAsync(
        //     new ECommerce.Services.Shared.Catalogs.Products.Events.Integration.ProductCreatedV1(
        //         notification.InternalCommandId,
        //         notification.Name,
        //         notification.Stock,
        //         notification.CategoryName ?? "",
        //         notification.Stock),
        //     null,
        //     cancellationToken);

        return Task.CompletedTask;
    }
}
