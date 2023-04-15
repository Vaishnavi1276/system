using BuildingBlocks.Core.CQRS.Events.Internal;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductPrice.v1;

public record ProductPriceChanged(decimal Price) : DomainEvent
{
    public static ProductPriceChanged Of(decimal price)
    {
        price.NotBeNegativeOrZero();

        return new ProductPriceChanged(price);
    }
}
