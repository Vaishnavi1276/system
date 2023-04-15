using BuildingBlocks.Core.Domain;

namespace ECommerce.Services.Catalogs.Categories;

// https://stackoverflow.com/a/32354885/581476
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public class Category : Aggregate<CategoryId>
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    public Category() { }

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Code { get; private set; } = default!;

    public static Category Create(CategoryId id, string name, string code, string description = "")
    {
        // input validation will do in the command and our value objects, here we just do business validation
        var category = new Category { Id = id };

        category.ChangeName(name);
        category.ChangeDescription(description);
        category.ChangeCode(code);

        return category;
    }

    public void ChangeName(string name)
    {
        // input validation will do in the command and our value objects, here we just do business validation
        Name = name;
    }

    public void ChangeCode(string code)
    {
        Code = code;
    }

    public void ChangeDescription(string description)
    {
        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} - {Code}";
    }
}
