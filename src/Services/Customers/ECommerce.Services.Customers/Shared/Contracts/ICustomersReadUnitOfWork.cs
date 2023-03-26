using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.Shared.Contracts;

public interface ICustomersReadUnitOfWork : IUnitOfWork<CustomersReadDbContext>
{
    public IRestockSubscriptionReadRepository RestockSubscriptionsRepository { get; }
    public ICustomerReadRepository CustomersRepository { get; }
}
