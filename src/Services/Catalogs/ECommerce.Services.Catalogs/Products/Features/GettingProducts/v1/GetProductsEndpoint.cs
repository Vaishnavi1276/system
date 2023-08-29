using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Problem.HttpResults;
using ECommerce.Services.Catalogs.Products.Dtos.v1;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProducts.v1;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
// https://im5tu.io/article/2022/09/asp.net-core-versioning-mvc-apis/
internal static class GetProductsEndpoint
{
    internal static RouteHandlerBuilder MapGetProductsByPageEndpoint(this IEndpointRouteBuilder app)
    {
        // return app.MapQueryEndpoint<GetProductsRequestParameters, GetProductsResponse, GetProducts,
        //         GetProductsResult>("/")
        return app.MapGet("/", Handle)
            // .RequireAuthorization()
            .WithTags(ProductsConfigs.Tag)
            .WithName(nameof(GetProducts))
            .WithSummaryAndDescription(nameof(GetProducts).Humanize(), nameof(GetProducts).Humanize())
            .WithDisplayName(nameof(GetProducts).Humanize())
            // .Produces<GetProductsResponse>("Products fetched successfully.", StatusCodes.Status200OK)
            // .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            // .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(1.0);

        async Task<Results<Ok<GetProductsResponse>, ValidationProblem, UnAuthorizedHttpProblemResult>> Handle(
            [AsParameters] GetProductsRequestParameters requestParameters
        )
        {
            var (context, queryProcessor, mapper, cancellationToken, _, _, _, _) = requestParameters;

            var query = GetProducts.Of(
                new PageRequest
                {
                    PageNumber = requestParameters.PageNumber,
                    PageSize = requestParameters.PageSize,
                    SortOrder = requestParameters.SortOrder,
                    Filters = requestParameters.SortOrder
                }
            );

            var result = await queryProcessor.SendAsync(query, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(new GetProductsResponse(result.Products));
        }
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#parameter-binding-for-argument-lists-with-asparameters
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-precedence
internal record GetProductsRequestParameters(
    HttpContext HttpContext,
    IQueryProcessor QueryProcessor,
    IMapper Mapper,
    CancellationToken CancellationToken,
    int PageSize = 10,
    int PageNumber = 1,
    string? Filters = null,
    string? SortOrder = null
) : IHttpQuery, IPageRequest;

internal record GetProductsResponse(IPageList<ProductDto> Products);
