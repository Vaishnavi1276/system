using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.v1;

public record GetRestockSubscriptionsResponse(IListResultModel<RestockSubscriptionDto> RestockSubscriptions);
