using ECommerce.Services.Catalogs.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Shared.Workers;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
public class MigrationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MigrationWorker> _logger;

    public MigrationWorker(IServiceScopeFactory serviceScopeFactory, ILogger<MigrationWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Migration worker started");

        using var serviceScope = _serviceScopeFactory.CreateScope();
        var catalogDbContext = serviceScope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        _logger.LogInformation("Updating catalog database...");

        await catalogDbContext.Database.MigrateAsync(cancellationToken: stoppingToken);

        _logger.LogInformation("catalog database Updated");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migration worker stopped");

        return base.StopAsync(cancellationToken);
    }
}
