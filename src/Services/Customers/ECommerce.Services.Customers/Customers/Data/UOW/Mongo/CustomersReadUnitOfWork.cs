using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.Customers.Data.UOW.Mongo;

public class CustomersReadUnitOfWork : MongoUnitOfWork<CustomersReadDbContext>, ICustomersReadUnitOfWork
{
    public CustomersReadUnitOfWork(
        CustomersReadDbContext context,
        IRestockSubscriptionReadRepository restockRepository,
        ICustomerReadRepository customerRepository
    )
        : base(context)
    {
        RestockSubscriptionsRepository = restockRepository;
        CustomersRepository = customerRepository;
    }

    public IRestockSubscriptionReadRepository RestockSubscriptionsRepository { get; }
    public ICustomerReadRepository CustomersRepository { get; }
}
