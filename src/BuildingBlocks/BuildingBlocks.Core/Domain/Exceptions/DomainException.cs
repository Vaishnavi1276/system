using BuildingBlocks.Core.Exception.Types;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.Domain.Exceptions;

/// <summary>
/// Exception type for domain exceptions.
/// </summary>
public class DomainException : CustomException
{
    public DomainException(string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
