using BuildingBlocks.Abstractions.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.Workers;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
// Hint: we can't guarantee execution order of our migration before our test so we should apply it manually in tests
public class MigrationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MigrationWorker> _logger;
    private readonly IWebHostEnvironment _environment;

    public MigrationWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MigrationWorker> logger,
        IWebHostEnvironment environment
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _environment = environment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_environment.IsEnvironment("test"))
        {
            // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
            // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var migrations = serviceScope.ServiceProvider.GetServices<IMigrationExecutor>();

            foreach (var migration in migrations)
            {
                _logger.LogInformation("Migration '{Migration}' started...", migrations.GetType().Name);
                await migration.ExecuteAsync(stoppingToken);
                _logger.LogInformation("Migration '{Migration}' ended...", migration.GetType().Name);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsEnvironment("test"))
        {
            _logger.LogInformation("Migration worker stopped");
        }

        return base.StopAsync(cancellationToken);
    }
}
