using System.Reflection;
using BuildingBlocks.Abstractions.Web.Problem;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Reflection;
using Microsoft.AspNetCore.Http;
using Scrutor;

namespace BuildingBlocks.Web.Problem;

// https://www.strathweb.com/2022/08/problem-details-responses-everywhere-with-asp-net-core-and-net-7/
public static class RegistrationExtensions
{
    public static IServiceCollection AddCustomProblemDetails(
        this IServiceCollection services,
        Action<ProblemDetailsOptions>? configure = null,
        params Assembly[] scanAssemblies
    )
    {
        var assemblies = scanAssemblies.Any()
            ? scanAssemblies
            : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly()).Distinct().ToArray();

        services.AddProblemDetails(configure);
        services.ReplaceSingleton<IProblemDetailsService, ProblemDetailsService>();
        // services.AddSingleton<IProblemDetailsWriter, ProblemDetailsWriter>();

        RegisterAllMappers(services, assemblies);

        return services;
    }

    private static void RegisterAllMappers(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IProblemDetailMapper)), false)
                    .UsingRegistrationStrategy(RegistrationStrategy.Append)
                    .As<IProblemDetailMapper>()
                    .WithLifetime(ServiceLifetime.Singleton)
        );
    }
}
