using BuildingBlocks.Core.Reflection.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.IntegrationTests.Fixtures;

public static class InMemoryStoreFactory<TContext>
    where TContext : DbContext
{
    public static TContext Create()
    {
        var dbContextOptions = new DbContextOptionsBuilder<TContext>().UseInMemoryDatabase(nameof(TContext)).Options;

        return typeof(TContext).CreateInstanceFromType<TContext>(dbContextOptions);
    }

    public static void Destroy(TContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
