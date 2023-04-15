using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Identity.Identity.Exceptions;

public class UserLockedException : AppException
{
    public UserLockedException(string userId)
        : base($"userId '{userId}' has been locked.", StatusCodes.Status403Forbidden) { }
}
