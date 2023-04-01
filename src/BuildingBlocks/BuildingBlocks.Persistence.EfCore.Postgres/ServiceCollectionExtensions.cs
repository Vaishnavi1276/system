using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore.Interceptors;
using BuildingBlocks.Core.Web.Extensions.ServiceCollection;
using Core.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresDbContext<TDbContext>(
        this IServiceCollection services,
        Assembly? migrationAssembly = null,
        Action<DbContextOptionsBuilder>? builder = null,
        params Assembly[] assembliesToScan
    )
        where TDbContext : DbContext, IDbFacadeResolver, IDomainEventContext
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddValidatedOptions<PostgresOptions>(nameof(PostgresOptions));

        services.AddScoped<IConnectionFactory>(sp =>
        {
            var postgresOptions = sp.GetService<PostgresOptions>();
            Guard.Against.NullOrEmpty(postgresOptions?.ConnectionString);
            return new NpgsqlConnectionFactory(postgresOptions.ConnectionString);
        });

        services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                var postgresOptions = sp.GetRequiredService<PostgresOptions>();

                options
                    .UseNpgsql(
                        postgresOptions.ConnectionString,
                        sqlOptions =>
                        {
                            var name =
                                migrationAssembly?.GetName().Name
                                ?? postgresOptions.MigrationAssembly
                                ?? typeof(TDbContext).Assembly.GetName().Name;

                            sqlOptions.MigrationsAssembly(name);
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    // https://github.com/efcore/EFCore.NamingConventions
                    .UseSnakeCaseNamingConvention();

                // ref: https://andrewlock.net/series/using-strongly-typed-entity-ids-to-avoid-primitive-obsession/
                options.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<long>>();

                options.AddInterceptors(
                    new AuditInterceptor(),
                    new SoftDeleteInterceptor(),
                    new ConcurrencyInterceptor()
                );

                builder?.Invoke(options);
            }
        );

        services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        services.AddScoped<IDomainEventContext>(provider => provider.GetService<TDbContext>()!);
        services.AddScoped<IDomainEventsAccessor, EfDomainEventAccessor>();

        services.AddPostgresRepositories(assembliesToScan);
        services.AddPostgresUnitOfWork(assembliesToScan);

        return services;
    }

    private static IServiceCollection AddPostgresRepositories(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Any() ? assembliesToScan : new[] { Assembly.GetCallingAssembly() };
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)), false)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
        );

        return services;
    }

    private static IServiceCollection AddPostgresUnitOfWork(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Any() ? assembliesToScan : new[] { Assembly.GetCallingAssembly() };
        services.Scan(
            scan =>
                scan.FromAssemblies(scanAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IEfUnitOfWork<>)), false)
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithTransientLifetime()
        );

        return services;
    }
}
