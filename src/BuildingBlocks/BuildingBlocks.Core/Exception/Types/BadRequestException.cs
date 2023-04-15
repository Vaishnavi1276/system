using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Exception.Types;

public class BadRequestException : CustomException
{
    public BadRequestException(string message, System.Exception? innerException = null)
        : base(message, StatusCodes.Status400BadRequest, innerException) { }
}
