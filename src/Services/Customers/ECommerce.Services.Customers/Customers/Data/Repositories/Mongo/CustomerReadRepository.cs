using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.Customers.Data.Repositories.Mongo;

public class CustomerReadRepository
    : MongoRepositoryBase<CustomersReadDbContext, Customer, Guid>,
        ICustomerReadRepository
{
    public CustomerReadRepository(CustomersReadDbContext context)
        : base(context) { }
}
