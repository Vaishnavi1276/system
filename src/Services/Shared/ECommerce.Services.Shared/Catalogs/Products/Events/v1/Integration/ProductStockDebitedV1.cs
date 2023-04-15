using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;

public record ProductStockDebitedV1(long ProductId, int NewStock, int DebitedQuantity) : IntegrationEvent
{
    public static ProductStockDebitedV1 Of(long productId, int newStock, int debitedQuantity)
    {
        productId.NotBeNegativeOrZero();
        newStock.NotBeNegativeOrZero();
        debitedQuantity.NotBeNegativeOrZero();

        return new ProductStockDebitedV1(productId, newStock, debitedQuantity);
    }
}
