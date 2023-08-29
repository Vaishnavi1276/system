using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Validation.Extensions;
using ECommerce.Services.Catalogs.Brands.Exceptions.Application;
using ECommerce.Services.Catalogs.Brands.ValueObjects;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Categories.Exceptions.Domain;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using ECommerce.Services.Catalogs.Suppliers;
using ECommerce.Services.Catalogs.Suppliers.Exceptions.Application;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
internal record CreateProduct(
    string Name,
    decimal Price,
    int Stock,
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
    string? Description = null,
    IEnumerable<CreateProductImageRequest>? Images = null
) : ITxCreateCommand<CreateProductResult>
{
    public long Id { get; } = SnowFlakIdGenerator.NewId();

    // Create product with inline validation.
    public static CreateProduct Of(
        string? name,
        decimal price,
        int stock,
        int restockThreshold,
        int maxStockThreshold,
        ProductStatus status,
        int width,
        int height,
        int depth,
        string? size,
        ProductColor color,
        long categoryId,
        long supplierId,
        long brandId,
        string? description = null,
        IEnumerable<CreateProductImageRequest>? images = null
    )
    {
        return new CreateProductValidator().HandleValidation(
            new CreateProduct(
                name!,
                price,
                stock,
                restockThreshold,
                maxStockThreshold,
                status,
                width,
                height,
                depth,
                size!,
                color,
                categoryId,
                supplierId,
                brandId,
                description,
                images
            )
        );
    }
}

internal class CreateProductValidator : AbstractValidator<CreateProduct>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0).WithMessage("Id must be greater than 0");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Price).NotEmpty().GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status is required.");
        RuleFor(x => x.Color).IsInEnum().WithMessage("Color is required.");
        RuleFor(x => x.Stock).NotEmpty().GreaterThan(0).WithMessage("Stock must be greater than 0");
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

internal class CreateProductHandler : ICommandHandler<CreateProduct, CreateProductResult>
{
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ICatalogDbContext _catalogDbContext;

    public CreateProductHandler(
        ICatalogDbContext catalogDbContext,
        IMapper mapper,
        ILogger<CreateProductHandler> logger
    )
    {
        _catalogDbContext = catalogDbContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CreateProductResult> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var (
            name,
            price,
            stock,
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
            description,
            imageItems
        ) = command;

        var images = imageItems
            ?.Select(
                x =>
                    new ProductImage(
                        EntityId.CreateEntityId(SnowFlakIdGenerator.NewId()),
                        x.ImageUrl,
                        x.IsMain,
                        ProductId.Of(command.Id)
                    )
            )
            .ToList();

        var category = await _catalogDbContext.FindCategoryAsync(CategoryId.Of(categoryId));
        if (category is null)
        {
            throw new CategoryDomainException(categoryId);
        }

        var brand = await _catalogDbContext.FindBrandAsync(BrandId.Of(brandId));
        if (brand is null)
        {
            throw new BrandNotFoundException(brandId);
        }

        var supplier = await _catalogDbContext.FindSupplierByIdAsync(SupplierId.Of(supplierId));
        if (supplier is null)
        {
            throw new SupplierNotFoundException(supplierId);
        }

        // await _domainEventDispatcher.DispatchAsync(cancellationToken, new Events.Domain.CreatingProduct());
        var product = Product.Create(
            ProductId.Of(command.Id),
            Name.Of(name),
            Stock.Of(stock, restockThreshold, maxStockThreshold),
            status,
            Dimensions.Of(width, height, depth),
            Size.Of(size),
            color,
            description,
            Price.Of(price),
            CategoryId.Of(categoryId),
            SupplierId.Of(supplierId),
            BrandId.Of(brandId),
            images
        );

        await _catalogDbContext.Products.AddAsync(product, cancellationToken: cancellationToken);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        var created = await _catalogDbContext.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .SingleOrDefaultAsync(x => x.Id == product.Id, cancellationToken: cancellationToken);

        _logger.LogInformation("Product a with ID: '{ProductId} created.'", command.Id);

        return new CreateProductResult(command.Id);
    }
}

internal record CreateProductResult(long Id);
