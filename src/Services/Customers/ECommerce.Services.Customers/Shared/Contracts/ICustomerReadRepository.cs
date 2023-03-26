using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Customers.Customers.Models.Reads;

namespace ECommerce.Services.Customers.Shared.Contracts;

public interface ICustomerReadRepository : IRepository<Customer, Guid> { }
