using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;

namespace ECommerce.Services.Customers.Shared.Contracts;

public interface IRestockSubscriptionReadRepository : IRepository<RestockSubscription, Guid> { }
