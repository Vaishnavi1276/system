using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Brands.Exceptions.Application;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Categories.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Features.GettingProductById.v1;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using ECommerce.Services.Catalogs.Suppliers;
using ECommerce.Services.Catalogs.Suppliers.Exceptions.Application;
using FluentValidation;
using MediatR;

namespace ECommerce.Services.Catalogs.Products.Features.UpdatingProduct.v1;

internal record UpdateProduct(
    long Id,
    string Name,
    decimal Price,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    int Width,
    int Height,
    int Depth,
    string Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description = null
) : ITxCommand
{
    public static UpdateProduct Of(
        long id,
        string? name,
        decimal price,
        int restockThreshold,
        int maxStockThreshold,
        ProductStatus status,
        int width,
        int height,
        int depth,
        string? size,
        long categoryId,
        long supplierId,
        long brandId,
        string? description = null
    )
    {
        return new UpdateProductValidator().HandleValidation(
            new UpdateProduct(
                id,
                name!,
                price,
                restockThreshold,
                maxStockThreshold,
                status,
                width,
                height,
                depth,
                size!,
                categoryId,
                supplierId,
                brandId,
                description
            )
        );
    }
}

internal class UpdateProductValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0).WithMessage("Id must be greater than 0");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Price).NotEmpty().GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status is required.");
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

internal class UpdateProductInvalidateCache : InvalidateCacheRequest<UpdateProduct>
{
    public override IEnumerable<string> CacheKeys(UpdateProduct request)
    {
        yield return $"{Prefix}{nameof(GetProductById)}_{request.Id}";
    }
}

internal class UpdateProductCommandHandler : ICommandHandler<UpdateProduct>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public UpdateProductCommandHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<Unit> Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var (
            id,
            name,
            price,
            restockThreshold,
            maxStockThreshold,
            productStatus,
            width,
            height,
            depth,
            size,
            categoryId,
            supplierId,
            brandId,
            description
        ) = command;

        var product = await _catalogDbContext.FindProductByIdAsync(ProductId.Of(id));
        if (product is null)
        {
            throw new ProductNotFoundException(id);
        }

        var category = await _catalogDbContext.FindCategoryAsync(CategoryId.Of(id));
        if (category is null)
            throw new CategoryNotFoundException(categoryId);

        var brand = await _catalogDbContext.FindBrandAsync(BrandId.Of(brandId));
        if (brand is null)
            throw new BrandNotFoundException(brandId);

        var supplier = await _catalogDbContext.FindSupplierByIdAsync(SupplierId.Of(supplierId));
        if (supplier is null)
            throw new SupplierNotFoundException(supplierId);

        product.ChangeCategory(CategoryId.Of(categoryId));
        product.ChangeBrand(BrandId.Of(brandId));
        product.ChangeSupplier(SupplierId.Of(supplierId));

        product.ChangeDescription(description);
        product.ChangeName(Name.Of(name));
        product.ChangePrice(Price.Of(price));
        product.ChangeSize(Size.Of(size));
        product.ChangeStatus(productStatus);
        product.ChangeDimensions(Dimensions.Of(width, height, depth));
        product.ChangeMaxStockThreshold(maxStockThreshold);
        product.ChangeRestockThreshold(restockThreshold);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
