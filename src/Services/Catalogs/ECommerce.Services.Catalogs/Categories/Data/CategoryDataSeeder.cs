using System.Globalization;
using AutoBogus;
using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Categories.Data;

public class CategoryDataSeeder : IDataSeeder
{
    private readonly ICatalogDbContext _dbContext;

    public CategoryDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _dbContext.Categories.AnyAsync())
            return;

        // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
        // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
        // https://github.com/bchavez/Bogus#bogus-api-support
        // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74

        // faker works with normal syntax because category has a default constructor
        var categories = new CategorySeedFaker().Generate(5);

        await _dbContext.Categories.AddRangeAsync(categories);
        await _dbContext.SaveChangesAsync();
    }

    public int Order => 1;
}

public sealed class CategorySeedFaker : Faker<Category>
{
    public CategorySeedFaker()
    {
        var categoryId = 1;

        CustomInstantiator(
            f =>
                Category.Of(
                    CategoryId.Of(categoryId++),
                    f.Commerce.Categories(1).First(),
                    f.Commerce.ProductDescription(),
                    f.Random.Number(1000, 5000).ToString(CultureInfo.InvariantCulture)
                )
        );
    }
}
