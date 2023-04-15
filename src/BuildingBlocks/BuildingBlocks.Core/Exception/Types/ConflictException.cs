using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class ConflictException : CustomException
{
    public ConflictException(string message, System.Exception? innerException = null)
        : base(message, StatusCodes.Status409Conflict, innerException) { }
}
