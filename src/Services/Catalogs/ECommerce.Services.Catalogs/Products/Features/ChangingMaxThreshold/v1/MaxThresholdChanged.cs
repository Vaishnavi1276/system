using BuildingBlocks.Core.CQRS.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingMaxThreshold.v1;

internal record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent
{
    public static MaxThresholdChanged Of(long productId, int maxThreshold)
    {
        productId.NotBeNegativeOrZero();
        maxThreshold.NotBeNegativeOrZero();

        return new MaxThresholdChanged(productId, maxThreshold);
    }
}
