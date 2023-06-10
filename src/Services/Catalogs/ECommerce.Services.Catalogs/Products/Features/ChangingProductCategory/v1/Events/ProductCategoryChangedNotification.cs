using BuildingBlocks.Core.Domain.Events.Internal;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory.v1.Events;

public record ProductCategoryChangedNotification(CategoryId CategoryId, ProductId ProductId) : DomainEvent;
