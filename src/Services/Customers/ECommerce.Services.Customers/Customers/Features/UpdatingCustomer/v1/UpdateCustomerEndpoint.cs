using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Minimal.Extensions;
using Humanizer;

namespace ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1;

internal class UpdateCustomerEndpoint : ICommandMinimalEndpoint<UpdateCustomerRequest, UpdateCustomerRequestParameters>
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPut("/", HandleAsync)
            .RequireAuthorization()
            .WithTags(CustomersConfigs.Tag)
            .RequireAuthorization()
            .WithName(nameof(UpdateCustomer))
            .WithDisplayName(nameof(UpdateCustomer).Humanize())
            .WithSummaryAndDescription(nameof(UpdateCustomer).Humanize(), nameof(UpdateCustomer).Humanize())
            // .Produces("Customer updated successfully.", StatusCodes.Status204NoContent)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem("UnAuthorized request.", StatusCodes.Status401Unauthorized)
            .MapToApiVersion(1.0);
    }

    public async Task<IResult> HandleAsync(UpdateCustomerRequestParameters requestParameters)
    {
        var (request, context, commandProcessor, mapper, cancellationToken) = requestParameters;

        var command = mapper.Map<UpdateCustomer>(request);

        await commandProcessor.SendAsync(command, cancellationToken);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
        return TypedResults.NoContent();
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record UpdateCustomerRequestParameters(
    [FromBody] UpdateCustomerRequest Request,
    HttpContext HttpContext,
    ICommandProcessor CommandProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken
) : IHttpCommand<UpdateCustomerRequest>;

// These parameters can be pass null from the user
internal sealed record UpdateCustomerRequest(
    long Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    DateTime? BirthDate = null,
    string? Nationality = null,
    string? Address = null
);
