using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record SupplierInformation
{
    // EF
    private SupplierInformation() { }

    public Name Name { get; private set; } = default!;
    public SupplierId Id { get; private set; } = default!;

    public static SupplierInformation Of(SupplierId? id, Name? name)
    {
        // validations should be placed here instead of constructor
        id.NotBeNull();
        name.NotBeNull();

        return new SupplierInformation { Id = id, Name = name };
    }

    public void Deconstruct(out Name name, out SupplierId supplierId) => (name, supplierId) = (Name, Id);
}
