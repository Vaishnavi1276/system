using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Persistence.EventStoreDB.Subscriptions;
using EventStore.Client;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Persistence.EventStoreDB.Extensions;

public static class RegistrationExtensions
{
    public static IServiceCollection AddEventStoreDb(
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime withLifetime = ServiceLifetime.Scoped,
        Action<EventStoreDbOptions>? configureOptions = null
    )
    {
        var eventStoreDbConfig = configuration.BindOptions<EventStoreDbOptions>();

        services.AddSingleton(
            new EventStoreClient(EventStoreClientSettings.Create(eventStoreDbConfig.ConnectionString))
        );

        services.AddEventSourcing<EventStoreDbEventStore>();

        if (eventStoreDbConfig.UseInternalCheckpointing)
        {
            services.AddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        if (configureOptions is { })
        {
            services.Configure(nameof(EventStoreDbOptions), configureOptions);
        }
        else
        {
            services
                .AddOptions<EventStoreDbOptions>()
                .Bind(configuration.GetSection(nameof(EventStoreDbOptions)))
                .ValidateDataAnnotations();
        }

        return services;
    }

    public static IServiceCollection AddEventStoreDbSubscriptionToAll(
        this IServiceCollection services,
        bool checkpointToEventStoreDb = true
    )
    {
        if (checkpointToEventStoreDb)
        {
            services.AddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        return services.AddHostedService<EventStoreDBSubscriptionToAll>();
    }
}
