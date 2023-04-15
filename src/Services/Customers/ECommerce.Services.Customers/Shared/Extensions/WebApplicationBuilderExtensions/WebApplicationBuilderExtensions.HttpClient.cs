using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.Extensions.ServiceCollection;
using BuildingBlocks.Resiliency;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomHttpClients(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedOptions<IdentityApiClientOptions>();
        builder.Services.AddValidatedOptions<CatalogsApiClientOptions>();
        builder.Services.AddValidatedOptions<PolicyOptions>();

        builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(
            (client, sp) =>
            {
                var catalogApiOptions = sp.GetRequiredService<IOptions<CatalogsApiClientOptions>>();
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>();
                catalogApiOptions.Value.NotBeNull();

                var baseAddress = catalogApiOptions.Value.BaseApiAddress;
                client.BaseAddress = new Uri(baseAddress);
                return new CatalogApiClient(client, catalogApiOptions, policyOptions);
            }
        );

        builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(
            (client, sp) =>
            {
                var identityApiOptions = sp.GetRequiredService<IOptions<IdentityApiClientOptions>>();
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>();
                identityApiOptions.Value.NotBeNull();

                var baseAddress = identityApiOptions.Value.BaseApiAddress;
                client.BaseAddress = new Uri(baseAddress);
                return new IdentityApiClient(client, identityApiOptions, policyOptions);
            }
        );

        return builder;
    }
}
