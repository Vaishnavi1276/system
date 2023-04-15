using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class NotFoundException : CustomException
{
    public NotFoundException(string message, System.Exception? innerException = null)
        : base(message, StatusCodes.Status404NotFound, innerException) { }
}
