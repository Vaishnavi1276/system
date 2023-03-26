using BuildingBlocks.Core.Persistence.EfCore;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Shared.Data;

public class CatalogDbContext : EfDbContextBase, ICatalogDbContext
{
    public const string DefaultSchema = "catalog";

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<ProductView> ProductsView { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Supplier> Suppliers { get; set; } = default!;
    public DbSet<Brand> Brands { get; set; } = default!;
}
