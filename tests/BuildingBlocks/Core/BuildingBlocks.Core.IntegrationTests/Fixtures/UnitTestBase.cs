using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.IntegrationTests.Fixtures;

public class UnitTestBase<TContext> : IDisposable
    where TContext : DbContext
{
    protected readonly TContext Context;

    protected UnitTestBase()
    {
        Context = InMemoryStoreFactory<TContext>.Create();
    }

    public void Dispose()
    {
        InMemoryStoreFactory<TContext>.Destroy(Context);
    }
}
