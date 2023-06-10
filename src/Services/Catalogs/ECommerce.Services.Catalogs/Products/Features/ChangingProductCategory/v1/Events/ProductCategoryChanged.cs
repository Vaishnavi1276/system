using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory.v1.Events;

internal record ProductCategoryChanged(long CategoryId, long ProductId) : DomainEvent
{
    public static ProductCategoryChanged Of(long categoryId, long productId)
    {
        categoryId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductCategoryChanged(categoryId, productId);
    }
}
