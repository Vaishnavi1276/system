using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Catalogs.Categories.Exceptions.Application;

public class CategoryNotFoundException : AppException
{
    public CategoryNotFoundException(long id)
        : base($"Category with id '{id}' not found.", StatusCodes.Status404NotFound) { }

    public CategoryNotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound) { }
}
