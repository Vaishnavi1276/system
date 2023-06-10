using System.Diagnostics;
using System.Reactive.Linq;
using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Core.Registrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tests.Shared.Helpers;
using Xunit.Abstractions;

namespace BuildingBlocks.Core.IntegrationTests.Fixtures;

public class IntegrationFixture : IAsyncLifetime
{
    private readonly CancellationTokenSource _cancellationTokenSource = new(TimeSpan.FromSeconds(10));

    public IntegrationFixture()
    {
        var services = new ServiceCollection();

        var configuration = ConfigurationHelper.BuildConfiguration();

        services.AddLogging();

        services.AddCore(configuration);
        services.AddCqrs();
        services.AddInMemoryTransport();
        services.AddCustomInMemoryCache(configuration);

        services.AddScoped<TextWriter>(_ => new StringWriter());

        ServiceProvider = services.BuildServiceProvider();
        Logger = ServiceProvider.GetRequiredService<ILogger<IntegrationFixture>>();
        ServiceProvider.StartHostedServices(CancellationToken);
        Checkpoint = new Checkpoint();
    }

    public IServiceProvider ServiceProvider { get; }
    public ILogger<IntegrationFixture> Logger { get; }
    public Checkpoint Checkpoint { get; }

    public async Task<ObservedMessageContexts> ExecuteAndWait<TMessage>(Func<Task> testAction, TimeSpan? timeout = null)
        where TMessage : IEvent
    {
        timeout ??= TimeSpan.FromSeconds(120);
        var taskCompletionSource = new TaskCompletionSource();

        var incomingMessages = new List<IEvent>();
        var outgoingMessages = new List<IEvent>();

        var obs = Observable.Empty<IEvent>();

        DiagnosticListener.AllListeners.Subscribe(
            delegate(DiagnosticListener listener)
            {
                // listen for 'MySampleLibrary' DiagnosticListener which inherits from abstract class DiagnosticSource
                if (listener.Name == OTelTransportOptions.InMemoryConsumerActivityName)
                {
                    //listen to specific event of listener
                    listener.Subscribe(
                        (pair) =>
                        {
                            if (pair.Key == OTelTransportOptions.Events.AfterProcessInMemoryMessage)
                            {
                                var incomingObs = listener.Select(e => e.Value)!.Cast<IEvent>();

                                incomingObs.Subscribe(incomingMessages.Add);
                                obs = obs.Merge(incomingObs);
                            }
                        }
                    );
                }

                if (listener.Name == OTelTransportOptions.InMemoryProducerActivityName)
                {
                    listener.Subscribe(
                        (pair) =>
                        {
                            if (pair.Key == OTelTransportOptions.Events.AfterSendInMemoryMessage)
                            {
                                var outgoingObs = listener.Select(e => e.Value)!.Cast<IEvent>();

                                outgoingObs.Subscribe(outgoingMessages.Add);
                                obs = obs.Merge(outgoingObs);
                            }
                        }
                    );
                }
            }
        );

        var finalObs = obs.Cast<TMessage>().TakeUntil(x => x.GetType() == typeof(TMessage));
        finalObs = finalObs.Timeout(timeout.Value);

        await testAction();

        // Force the observable to complete
        await finalObs.LastOrDefaultAsync();

        return new ObservedMessageContexts(incomingMessages, outgoingMessages);
    }

    public TaskCompletionSource<TMessage?> EnsureReceivedMessageToConsumer<TMessage>(TimeSpan? timeout = null)
        where TMessage : IEvent
    {
        var taskCompletionSource = new TaskCompletionSource<TMessage?>();

        DiagnosticListener.AllListeners.Subscribe(
            delegate(DiagnosticListener listener)
            {
                if (listener.Name == OTelTransportOptions.InMemoryConsumerActivityName)
                {
                    //listen to specific event of listener
                    listener.Subscribe(
                        (pair) =>
                        {
                            if (pair.Key == OTelTransportOptions.Events.AfterProcessInMemoryMessage)
                            {
                                AfterProcessMessage? afterProcess =
                                    pair.Value?.GetType().GetProperty("Payload")?.GetValue(pair.Value)
                                    as AfterProcessMessage;

                                if (
                                    afterProcess?.EventData != null
                                    && typeof(TMessage) == afterProcess?.EventData.GetType()
                                )
                                    taskCompletionSource.TrySetResult((TMessage)afterProcess.EventData);
                            }
                        }
                    );
                }

                if (listener.Name == OTelTransportOptions.InMemoryProducerActivityName)
                {
                    listener.Subscribe(
                        (pair) =>
                        {
                            if (pair.Key == OTelTransportOptions.Events.AfterSendInMemoryMessage)
                            {
                                AfterSendMessage? sentMessage =
                                    pair.Value?.GetType().GetProperty("Payload")?.GetValue(pair.Value)
                                    as AfterSendMessage;
                                if (
                                    sentMessage?.EventData != null
                                    && typeof(TMessage) == sentMessage?.EventData.GetType()
                                ) { }
                            }

                            if (pair.Key == OTelTransportOptions.Events.NoSubscriberToPublish)
                            {
                                NoSubscriberToPublishMessage? noSubscriberToPublishMessage =
                                    pair.Value?.GetType().GetProperty("Payload")?.GetValue(pair.Value)
                                    as NoSubscriberToPublishMessage;

                                if (
                                    noSubscriberToPublishMessage?.EventData != null
                                    && typeof(TMessage) == noSubscriberToPublishMessage?.EventData.GetType()
                                )
                                    taskCompletionSource.TrySetResult(
                                        (TMessage)noSubscriberToPublishMessage?.EventData
                                    );
                            }
                        }
                    );
                }
            }
        );

        Observable.Interval(timeout ?? TimeSpan.FromSeconds(30)).Subscribe(_ => taskCompletionSource.TrySetCanceled());

        return taskCompletionSource;
    }

    public void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        loggerFactory.AddXUnit(outputHelper);
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        List<Activity> exportActivities = new();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("MicroBootstrarp", "Samples.SampleServer")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("inmemory-test"))
            .AddInMemoryExporter(exportActivities)
            .Build();

        using var scope = ServiceProvider.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        List<Activity> exportActivities = new();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("MicroBootstrarp", "Samples.SampleServer")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("inmemory-test"))
            .AddInMemoryExporter(exportActivities)
            .Build();

        using var scope = ServiceProvider.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public class ObservedMessageContexts
    {
        public IList<IEvent> IncomingMessage { get; }
        public IList<IEvent> OutgoingMessage { get; }

        public ObservedMessageContexts(IList<IEvent> incomingMessage, IList<IEvent> outgoingMessage)
        {
            IncomingMessage = incomingMessage;
            OutgoingMessage = outgoingMessage;
        }
    }
}
