using BuildingBlocks.Core.Extensions.ServiceCollection;
using ECommerce.Services.Customers.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Services.Customers.DependencyTests;

public class DependencyTests
{
    [Fact]
    public void validate_service_dependencies()
    {
        var factory = new WebApplicationFactory<CustomersApiMetadata>().WithWebHostBuilder(webHostBuilder =>
        {
            webHostBuilder.UseEnvironment("test");

            webHostBuilder.ConfigureTestServices(services =>
            {
                services.AddTransient<IServiceCollection>(_ => services);
            });
        });

        using var scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var services = sp.GetRequiredService<IServiceCollection>();
        sp.ValidateDependencies(services, typeof(CustomersApiMetadata).Assembly, typeof(CustomersMetadata).Assembly);
    }
}
