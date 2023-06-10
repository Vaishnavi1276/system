using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductBrand.v1.Events.Domain;

internal record ProductBrandChanged(long BrandId, long ProductId) : DomainEvent
{
    public static ProductBrandChanged Of(long brandId, long productId)
    {
        brandId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductBrandChanged(brandId, productId);
    }
}
