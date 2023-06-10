using System.Reflection;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Persistence.Marten.Subscriptions;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weasel.Core;

namespace BuildingBlocks.Persistence.Marten.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMartenDB(
        this IServiceCollection services,
        Action<StoreOptions>? configureOptions = null,
        params Assembly[] scanAssemblies
    )
    {
        var assembliesToScan = scanAssemblies.Any() ? scanAssemblies : new[] { Assembly.GetCallingAssembly(), };

        services.AddValidatedOptions<MartenOptions>();

        services.AddEventSourcing<MartenEventStore>(ServiceLifetime.Scoped, assembliesToScan);

        services
            .AddMarten(sp =>
            {
                var martenDbOptions = sp.GetRequiredService<IOptions<MartenOptions>>().Value;

                var options = new StoreOptions();
                options.Connection(martenDbOptions.ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

                var schemaName = Environment.GetEnvironmentVariable("SchemaName");
                options.Events.DatabaseSchemaName = schemaName ?? martenDbOptions.WriteModelSchema;
                options.DatabaseSchemaName = schemaName ?? martenDbOptions.ReadModelSchema;

                options.UseDefaultSerialization(
                    EnumStorage.AsString,
                    nonPublicMembersStorage: NonPublicMembersStorage.All
                );

                options.Projections.Add(
                    new MartenSubscription(
                        new[] { new MartenEventPublisher(sp.GetRequiredService<IMediator>()) },
                        sp.GetRequiredService<ILogger<MartenSubscription>>()
                    ),
                    ProjectionLifecycle.Async,
                    "MartenSubscription"
                );

                if (martenDbOptions.UseMetadata)
                {
                    options.Events.MetadataConfig.CausationIdEnabled = true;
                    options.Events.MetadataConfig.CorrelationIdEnabled = true;
                    options.Events.MetadataConfig.HeadersEnabled = true;
                }

                configureOptions?.Invoke(options);

                return options;
            })
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup()
            //.OptimizeArtifactWorkflow()
            .AddAsyncDaemon(DaemonMode.Solo);

        return services;
    }
}
