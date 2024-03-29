using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.Products.Exceptions;

public class ProductNotFoundException : AppException
{
    public ProductNotFoundException(long id)
        : base($"Product with id {id} not found", HttpStatusCode.NotFound) { }
}
