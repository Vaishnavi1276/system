using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;

public record ProductStockReplenishedV1(long ProductId, int NewStock, int ReplenishedQuantity) : IntegrationEvent
{
    public static ProductStockReplenishedV1 Of(long productId, int newStock, int replenishedQuantity)
    {
        productId.NotBeNegativeOrZero();
        newStock.NotBeNegativeOrZero();
        replenishedQuantity.NotBeNegativeOrZero();

        return new ProductStockReplenishedV1(productId, newStock, replenishedQuantity);
    }
}
