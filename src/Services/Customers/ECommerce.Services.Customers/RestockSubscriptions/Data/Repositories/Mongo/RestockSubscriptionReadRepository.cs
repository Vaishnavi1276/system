using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.RestockSubscriptions.Data.Repositories.Mongo;

public class RestockSubscriptionReadRepository
    : MongoRepositoryBase<CustomersReadDbContext, RestockSubscription, Guid>,
        IRestockSubscriptionReadRepository
{
    public RestockSubscriptionReadRepository(CustomersReadDbContext context)
        : base(context) { }
}
