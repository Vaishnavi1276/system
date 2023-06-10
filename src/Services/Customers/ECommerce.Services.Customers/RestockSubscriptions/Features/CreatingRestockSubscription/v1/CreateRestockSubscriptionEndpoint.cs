using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Extensions;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public class CreateRestockSubscriptionEndpoint : ICommandMinimalEndpoint<CreateRestockSubscriptionRequest>
{
    public string GroupName => RestockSubscriptionsConfigs.Tag;
    public string PrefixRoute => RestockSubscriptionsConfigs.RestockSubscriptionsUrl;
    public double Version => 1.0;

    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        return builder
            .MapPost("/", HandleAsync)
            .AllowAnonymous()
            .Produces<CreateRestockSubscriptionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName("CreateRestockSubscription")
            .WithDisplayName("Register New RestockSubscription for Customer.");
    }

    public async Task<IResult> HandleAsync(
        HttpContext context,
        CreateRestockSubscriptionRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        request.NotBeNull();

        var command = new CreateRestockSubscription(request.CustomerId, request.ProductId, request.Email);

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(CreateRestockSubscriptionEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("RestockSubscriptionId", command.Id))
        {
            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.Created(
                $"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/{result.RestockSubscriptionId}",
                result
            );
        }
    }
}
