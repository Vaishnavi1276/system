using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Customers.Customers.Dtos.v1;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomers.v1;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
internal class GetCustomersEndpoint
    : EndpointBaseAsync.WithRequest<GetCustomersRequest?>.WithActionResult<GetCustomersResult>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetCustomersEndpoint(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    // We could use `SwaggerResponse` form `Swashbuckle.AspNetCore` package instead of `ProducesResponseType` for supporting custom description for status codes
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Customers response retrieved successfully (Success).",
        typeof(GetCustomersResult)
    )]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(StatusCodeProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input (Bad Request)", typeof(StatusCodeProblemDetails))]
    [SwaggerOperation(
        Summary = "Getting All Customers",
        Description = "Getting All Customers",
        OperationId = "GetCustomers",
        Tags = new[] { CustomersConfigs.Tag }
    )]
    [HttpGet(CustomersConfigs.CustomersPrefixUri, Name = "GetCustomers")]
    [ApiVersion(1.0)]
    public override async Task<ActionResult<GetCustomersResult>> HandleAsync(
        [FromQuery] GetCustomersRequest? request,
        CancellationToken cancellationToken = default
    )
    {
        Guard.Against.Null(request, nameof(request));

        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetCustomersEndpoint)))
        {
            var result = await _queryProcessor.SendAsync(
                new GetCustomers
                {
                    Filters = request.Filters,
                    Includes = request.Includes,
                    Page = request.Page,
                    Sorts = request.Sorts,
                    PageSize = request.PageSize
                },
                cancellationToken
            );
            var response = new GetCustomersResponse(result.Customers);

            return Ok(response);
        }
    }
}

// https://blog.codingmilitia.com/2022/01/03/getting-complex-type-as-simple-type-query-string-aspnet-core-api-controller/
// https://benfoster.io/blog/minimal-apis-custom-model-binding-aspnet-6/
internal record GetCustomersRequest : PageRequest
{
    // // For handling in minimal api
    // public static ValueTask<GetCustomersRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    // {
    //     var page = httpContext.Request.Query.Get<int>("Page", 1);
    //     var pageSize = httpContext.Request.Query.Get<int>("PageSize", 20);
    //     var customerState = httpContext.Request.Query.Get<CustomerState>("CustomerState", CustomerState.None);
    //     var sorts = httpContext.Request.Query.GetCollection<List<string>>("Sorts");
    //     var filters = httpContext.Request.Query.GetCollection<List<FilterModel>>("Filters");
    //     var includes = httpContext.Request.Query.GetCollection<List<string>>("Includes");
    //
    //     var request = new GetCustomersRequest()
    //     {
    //         Page = page,
    //         PageSize = pageSize,
    //         CustomerState = customerState,
    //         Sorts = sorts,
    //         Filters = filters,
    //         Includes = includes
    //     };
    //
    //     return ValueTask.FromResult<GetCustomersRequest?>(request);
    // }
}

internal record GetCustomersResponse(IListResultModel<CustomerReadDto> Customers);
