using BuildingBlocks.Core.Domain;

namespace ECommerce.Services.Catalogs.Brands;

public class Brand : Aggregate<BrandId>
{
    public string Name { get; private set; } = null!;

    public static Brand Create(BrandId id, string name)
    {
        // input validation will do in the command and our value objects, here we just do business validation
        var brand = new Brand { Id = id, };

        brand.ChangeName(name);

        return brand;
    }

    public void ChangeName(string name)
    {
        Name = name;
    }
}
