using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;

public record CustomerCreatedV1(long CustomerId) : IntegrationEvent
{
    public static CustomerCreatedV1 Of(long customerId) => new CustomerCreatedV1(customerId.NotBeNegativeOrZero());
}
