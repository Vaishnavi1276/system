using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ECommerce.Services.Customers.Shared.Data;

public class CustomersReadDbContext : MongoDbContext
{
    public CustomersReadDbContext(IOptions<MongoOptions> options)
        : base(options.Value)
    {
        RestockSubscriptions = GetCollection<RestockSubscription>();
        Customers = GetCollection<Customer>();
    }

    public IMongoCollection<RestockSubscription> RestockSubscriptions { get; }
    public IMongoCollection<Customer> Customers { get; }
}
