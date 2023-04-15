using System.Collections.Immutable;
using BuildingBlocks.Core.Domain;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.Dtos.v1;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;
using ECommerce.Services.Catalogs.Products.Features.ChangingMaxThreshold.v1;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductBrand.v1.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory.v1.Events;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductPrice.v1;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductSupplier.v1.Events;
using ECommerce.Services.Catalogs.Products.Features.ChangingRestockThreshold.v1;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.DebitingProductStock.v1.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.v1.Events.Domain;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.Models;

// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://enterprisecraftsmanship.com/posts/link-to-an-aggregate-reference-or-id/
// https://ardalis.com/avoid-collections-as-properties/?utm_sq=grcpqjyka3
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public class Product : Aggregate<ProductId>
{
    private List<ProductImage> _images = new();

    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    public Product() { }

    public Name Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Price Price { get; private set; } = default!;
    public ProductColor Color { get; private set; }
    public ProductStatus ProductStatus { get; private set; }
    public Category? Category { get; private set; } = default!;
    public CategoryId CategoryId { get; private set; } = default!;
    public SupplierId SupplierId { get; private set; } = default!;
    public Supplier? Supplier { get; private set; } = default!;
    public BrandId BrandId { get; private set; } = default!;
    public Brand? Brand { get; private set; } = default!;
    public Size Size { get; private set; } = default!;
    public Stock Stock { get; set; } = default!;
    public Dimensions Dimensions { get; private set; } = default!;
    public IReadOnlyList<ProductImage> Images => _images;

    public static Product Create(
        ProductId id,
        Name name,
        Stock stock,
        ProductStatus status,
        Dimensions dimensions,
        Size size,
        ProductColor color,
        string? description,
        Price price,
        CategoryId categoryId,
        SupplierId supplierId,
        BrandId brandId,
        IList<ProductImage>? images = null
    )
    {
        // input validation will do in the command and our value objects, here we just do business validation
        var product = new Product { Id = id, Stock = stock };

        var (available, restockThreshold, maxStockThreshold) = stock;
        var (width, height, depth) = dimensions;

        product.ChangeName(name);
        product.ChangeSize(size);
        product.ChangeDescription(description);
        product.ChangePrice(price);
        product.AddProductImages(images);
        product.ChangeStatus(status);
        product.ChangeColor(color);
        product.ChangeDimensions(dimensions);
        product.ChangeCategory(categoryId);
        product.ChangeBrand(brandId);
        product.ChangeSupplier(supplierId);

        product.AddDomainEvents(
            ProductCreated.Of(
                product.Id,
                product.Name.Value,
                product.Price.Value,
                available,
                restockThreshold,
                maxStockThreshold,
                product.ProductStatus,
                width,
                height,
                depth,
                product.Size,
                product.Color,
                product.CategoryId,
                product.SupplierId,
                product.BrandId,
                DateTime.Now,
                product.Description,
                product.Images.Select(x => new ProductImageDto(x.Id, x.ProductId, x.ImageUrl, x.IsMain))
            )
        );

        return product;
    }

    public void ChangeStatus(ProductStatus status)
    {
        ProductStatus = status;
    }

    public void ChangeDimensions(Dimensions dimensions)
    {
        Dimensions = dimensions;
    }

    public void ChangeSize(Size size)
    {
        Size = size;
    }

    public void ChangeColor(ProductColor color)
    {
        Color = color;
    }

    /// <summary>
    /// Sets catalog item name.
    /// </summary>
    /// <param name="name">The name to be changed.</param>
    public void ChangeName(Name name)
    {
        Name = name;
    }

    /// <summary>
    /// Sets catalog item description.
    /// </summary>
    /// <param name="description">The description to be changed.</param>
    public void ChangeDescription(string? description)
    {
        Description = description;
    }

    /// <summary>
    /// Sets catalog item price.
    /// </summary>
    /// <remarks>
    /// Raise a <see cref="ProductPriceChanged"/>.
    /// </remarks>
    /// <param name="price">The price to be changed.</param>
    public void ChangePrice(Price price)
    {
        if (Price == price)
            return;

        Price = price;

        AddDomainEvents(ProductPriceChanged.Of(price));
    }

    /// <summary>
    /// Decrements the quantity of a particular item in inventory and ensures the restockThreshold hasn't
    /// been breached. If so, a RestockRequest is generated in CheckThreshold.
    /// </summary>
    /// <param name="quantity">The number of items to debit.</param>
    /// <returns>int: Returns the number actually removed from stock. </returns>
    public int DebitStock(int quantity)
    {
        if (quantity < 0)
            quantity *= -1;

        if (HasStock(quantity) == false)
        {
            throw new InsufficientStockException(
                $"Empty stock, product item '{Name}' with quantity '{quantity}' is not available."
            );
        }

        var (available, restockThreshold, maxStockThreshold) = Stock;

        int removed = Math.Min(quantity, available);

        Stock = Stock.Of(available - removed, restockThreshold, maxStockThreshold);

        var (newAvailable, newRestockThreshold, newMaxStockThreshold) = Stock;

        if (newAvailable <= newRestockThreshold)
        {
            AddDomainEvents(
                ProductRestockThresholdReached.Of(Id, newAvailable, newRestockThreshold, newMaxStockThreshold, quantity)
            );
        }

        AddDomainEvents(ProductStockDebited.Of(Id, newAvailable, newRestockThreshold, newMaxStockThreshold, quantity));

        return removed;
    }

    /// <summary>
    /// Increments the quantity of a particular item in inventory.
    /// </summary>
    /// <returns>int: Returns the quantity that has been added to stock.</returns>
    /// <param name="quantity">The number of items to Replenish.</param>
    public Stock ReplenishStock(int quantity)
    {
        var (available, restockThreshold, maxStockThreshold) = Stock;

        // we don't have enough space in the inventory
        if (available + quantity > maxStockThreshold)
        {
            throw new MaxStockThresholdReachedException(
                $"Max stock threshold has been reached. Max stock threshold is {maxStockThreshold}"
            );
        }

        Stock = Stock.Of(available + quantity, restockThreshold, maxStockThreshold);

        var (newAvailable, newRestockThreshold, newMaxStockThreshold) = Stock;

        AddDomainEvents(
            ProductStockReplenished.Of(Id, newAvailable, newRestockThreshold, newMaxStockThreshold, quantity)
        );

        return Stock;
    }

    public Stock ChangeMaxStockThreshold(int newMaxStockThreshold)
    {
        var (available, restockThreshold, maxStockThreshold) = Stock;
        Stock = Stock.Of(available, restockThreshold, maxStockThreshold);

        AddDomainEvents(MaxThresholdChanged.Of(Id, maxStockThreshold));

        return Stock;
    }

    public Stock ChangeRestockThreshold(int restockThreshold)
    {
        Stock = Stock.Of(Stock.Available, restockThreshold, Stock.MaxStockThreshold);

        AddDomainEvents(RestockThresholdChanged.Of(Id, restockThreshold));

        return Stock;
    }

    public bool HasStock(int quantity)
    {
        return Stock.Available >= quantity;
    }

    public void Activate() => ChangeStatus(ProductStatus.Available);

    public void DeActive() => ChangeStatus(ProductStatus.Unavailable);

    /// <summary>
    /// Sets category.
    /// </summary>
    /// <param name="categoryId">The categoryId to be changed.</param>
    public void ChangeCategory(CategoryId categoryId)
    {
        CategoryId = categoryId;

        // add event to domain events list that will be raise during commiting transaction
        AddDomainEvents(ProductCategoryChanged.Of(categoryId, Id));
    }

    /// <summary>
    /// Sets supplier.
    /// </summary>
    /// <param name="supplierId">The supplierId to be changed.</param>
    public void ChangeSupplier(SupplierId supplierId)
    {
        SupplierId = supplierId;

        AddDomainEvents(ProductSupplierChanged.Of(supplierId, Id));
    }

    /// <summary>
    ///  Sets brand.
    /// </summary>
    /// <param name="brandId">The brandId to be changed.</param>
    public void ChangeBrand(BrandId brandId)
    {
        BrandId = brandId;

        AddDomainEvents(ProductBrandChanged.Of(brandId, Id));
    }

    public void AddProductImages(IList<ProductImage>? productImages)
    {
        if (productImages is null)
        {
            _images = null!;
            return;
        }

        _images.AddRange(productImages);
    }

    public void Deconstruct(
        out long id,
        out string name,
        out int availableStock,
        out int restockThreshold,
        out int maxStockThreshold,
        out ProductStatus status,
        out int width,
        out int height,
        out int depth,
        out string size,
        out ProductColor color,
        out string? description,
        out decimal price,
        out long categoryId,
        out long supplierId,
        out long brandId,
        out IList<ProductImage>? images
    ) =>
        (
            id,
            name,
            availableStock,
            restockThreshold,
            maxStockThreshold,
            status,
            width,
            height,
            depth,
            size,
            color,
            description,
            price,
            categoryId,
            supplierId,
            brandId,
            images
        ) = (
            Id,
            Name,
            Stock.Available,
            Stock.RestockThreshold,
            Stock.MaxStockThreshold,
            ProductStatus,
            Dimensions.Width,
            Dimensions.Height,
            Dimensions.Depth,
            Size,
            Color,
            Description,
            Price.Value,
            CategoryId,
            SupplierId,
            BrandId,
            Images.ToImmutableList()
        );
}
