using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BuildingBlocks.Core.IntegrationTests.Fixtures;

public class IntegrationTestBase : IClassFixture<IntegrationFixture>
{
    protected readonly CancellationTokenSource CancellationTokenSource = new(TimeSpan.FromSeconds(10));
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IServiceScope Scope;
    protected readonly IntegrationFixture Fixture;

    protected IntegrationTestBase(IntegrationFixture integrationFixture, ITestOutputHelper outputHelper)
    {
        Fixture = integrationFixture;
        Fixture.SetOutputHelper(outputHelper);
        ServiceProvider = Fixture.ServiceProvider;
        Scope = ServiceProvider.CreateScope();
    }

    public CancellationToken CancellationToken => CancellationTokenSource.Token;
    public IEventStore EventStore => Scope.ServiceProvider.GetRequiredService<IEventStore>();
    public IAggregateStore AggregateStore => Scope.ServiceProvider.GetRequiredService<IAggregateStore>();

    public IDomainEventPublisher DomainEventPublisher =>
        Scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

    public IMessagePublisher MessagePublisher => Scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

    protected ICommandProcessor CommandProcessor => Scope.ServiceProvider.GetRequiredService<ICommandProcessor>();
    protected IQueryProcessor QueryProcessor => Scope.ServiceProvider.GetRequiredService<IQueryProcessor>();
    protected IEventProcessor EventProcessor => Scope.ServiceProvider.GetRequiredService<IEventProcessor>();
    protected TextWriter TextWriter => Scope.ServiceProvider.GetRequiredService<TextWriter>();

    protected ILogger<IntegrationTestBase> Logger =>
        Scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase>>();

    protected OutboxMessagesHelper OutboxMessagesHelper =>
        Scope.ServiceProvider.GetRequiredService<OutboxMessagesHelper>();
}
