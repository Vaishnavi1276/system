using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.Shared.Data;
using Sieve.Services;

namespace ECommerce.Services.Customers.Customers.Data.Repositories.Mongo;

public class CustomerReadRepository
    : MongoRepositoryBase<CustomersReadDbContext, Customer, Guid>,
        ICustomerReadRepository
{
    public CustomerReadRepository(CustomersReadDbContext context, ISieveProcessor sieveProcessor)
        : base(context, sieveProcessor) { }
}
