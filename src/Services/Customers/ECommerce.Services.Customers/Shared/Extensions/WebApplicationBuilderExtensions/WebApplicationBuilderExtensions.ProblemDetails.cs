using BuildingBlocks.Web.Problem;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAppProblemDetails(this WebApplicationBuilder builder)
    {
        builder.Services.AddCustomProblemDetails(problemDetailsOptions =>
        {
            // customization problem details should go here
            problemDetailsOptions.CustomizeProblemDetails = problemDetailContext =>
            {
                // with help of capture exception middleware for capturing actual exception
                if (problemDetailContext.HttpContext.Features.Get<IExceptionHandlerFeature>() is { } exceptionFeature)
                { }
            };
        });

        return builder;
    }
}
