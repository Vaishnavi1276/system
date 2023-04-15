using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Core.CQRS.Events.Internal;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using ECommerce.Services.Catalogs.Products.Dtos.v1;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
internal record ProductCreated(
    long Id,
    string Name,
    decimal Price,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    int Width,
    int Height,
    int Depth,
    string Size,
    ProductColor Color,
    long CategoryId,
    long SupplierId,
    long BrandId,
    DateTime CreatedAt,
    string? Description = null,
    IEnumerable<ProductImageDto>? Images = null
) : DomainEvent
{
    public static ProductCreated Of(
        long id,
        string? name,
        decimal price,
        int availableStock,
        int restockThreshold,
        int maxStockThreshold,
        ProductStatus status,
        int width,
        int height,
        int depth,
        string size,
        ProductColor color,
        long categoryId,
        long supplierId,
        long brandId,
        DateTime createdAt,
        string? description = null,
        IEnumerable<ProductImageDto>? images = null
    )
    {
        return new ProductCreatedValidator().HandleValidation(
            new ProductCreated(
                id,
                name!,
                price,
                availableStock,
                restockThreshold,
                maxStockThreshold,
                status,
                width,
                height,
                depth,
                size,
                color,
                categoryId,
                supplierId,
                brandId,
                createdAt,
                description,
                images
            )
        );

        // // Also if validation rules are simple we can just validate inputs explicitly
        // id.NotBeEmpty();
        // name.NotBeNullOrWhiteSpace();
        // return new ProductCreated(
        //     id,
        //     name,
        //     price,
        //     availableStock,
        //     restockThreshold,
        //     maxStockThreshold,
        //     status,
        //     width,
        //     height,
        //     depth,
        //     size,
        //     color,
        //     categoryId,
        //     supplierId,
        //     brandId,
        //     createdAt,
        //     description,
        //     images);
    }
}

internal class ProductCreatedValidator : AbstractValidator<ProductCreated>
{
    public ProductCreatedValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0).WithMessage("Id must be greater than 0");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Price).NotEmpty().GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status is required.");
        RuleFor(x => x.Color).IsInEnum().WithMessage("Color is required.");
        RuleFor(x => x.AvailableStock).NotEmpty().GreaterThan(0).WithMessage("Stock must be greater than 0");
        RuleFor(x => x.MaxStockThreshold)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("MaxStockThreshold must be greater than 0");
        RuleFor(x => x.RestockThreshold)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("RestockThreshold must be greater than 0");
        RuleFor(x => x.CategoryId).NotEmpty().GreaterThan(0).WithMessage("CategoryId must be greater than 0");
        RuleFor(x => x.SupplierId).NotEmpty().GreaterThan(0).WithMessage("SupplierId must be greater than 0");
        RuleFor(x => x.BrandId).NotEmpty().GreaterThan(0).WithMessage("BrandId must be greater than 0");
    }
}

internal class ProductCreatedHandler : IDomainEventHandler<ProductCreated>
{
    private readonly CatalogDbContext _dbContext;

    public ProductCreatedHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        var existed = await _dbContext.ProductsView.FirstOrDefaultAsync(
            x => x.ProductId == notification.Id,
            cancellationToken
        );

        if (existed is null)
        {
            var product = await _dbContext.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.Supplier)
                .SingleOrDefaultAsync(x => x.Id == notification.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(notification.Id);
            }

            var productView = new ProductView
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name ?? string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
            };

            await _dbContext.Set<ProductView>().AddAsync(productView, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

// Mapping domain event to integration event in domain event handler is better from mapping in command handler (for preserving our domain rule invariants).
internal class ProductCreatedDomainEventToIntegrationMappingHandler : IDomainEventHandler<ProductCreated>
{
    public ProductCreatedDomainEventToIntegrationMappingHandler() { }

    public Task Handle(ProductCreated domainEvent, CancellationToken cancellationToken)
    {
        // 1. Mapping DomainEvent To IntegrationEvent
        // 2. Save Integration Event to Outbox
        return Task.CompletedTask;
    }
}
