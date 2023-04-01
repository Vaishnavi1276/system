using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;

internal class CreateCustomerEndpoint : ICommandMinimalEndpoint<CreateCustomerRequest>
{
    public string GroupName => CustomersConfigs.Tag;
    public string PrefixRoute => CustomersConfigs.CustomersPrefixUri;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPost("/", HandleAsync)
            .AllowAnonymous()
            .Produces<CreateCustomerResult>(StatusCodes.Status201Created)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .WithMetadata(new SwaggerOperationAttribute("Creating a Customer", "Creating a Customer"))
            .WithName("CreateCustomer")
            .WithDisplayName("Register New Customer.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        CreateCustomerRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        Guard.Against.Null(request, nameof(request));

        var command = new CreateCustomer(request.Email);

        var result = await commandProcessor.SendAsync(command, cancellationToken);
        var response = new CreateCustomerResponse(result.CustomerId, result.IdentityUserId);

        return Results.Created($"{CustomersConfigs.CustomersPrefixUri}/{result.CustomerId}", response);
    }
}

public record CreateCustomerRequest(string Email);

public record CreateCustomerResponse(long CustomerId, Guid IdentityUserId);
