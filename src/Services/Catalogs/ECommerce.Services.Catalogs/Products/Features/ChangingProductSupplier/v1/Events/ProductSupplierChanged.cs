using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductSupplier.v1.Events;

internal record ProductSupplierChanged(long SupplierId, long ProductId) : DomainEvent
{
    public static ProductSupplierChanged Of(long supplierId, long productId)
    {
        supplierId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductSupplierChanged(supplierId, productId);
    }
}
