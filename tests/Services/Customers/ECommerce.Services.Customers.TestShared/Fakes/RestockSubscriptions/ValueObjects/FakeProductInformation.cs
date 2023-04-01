using AutoBogus;
using ECommerce.Services.Customers.Products;
using ECommerce.Services.Customers.RestockSubscriptions.ValueObjects;

namespace ECommerce.Services.Customers.TestShared.Fakes.RestockSubscriptions.ValueObjects;

public sealed class FakeProductInformation : AutoFaker<ProductInformation>
{
    public FakeProductInformation()
    {
        RuleFor(x => x.Id, f => ProductId.Of(f.Random.Number(1, 100)));
    }
}
