using AutoBogus;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;

namespace ECommerce.Services.Customers.TestShared.Fakes.Customers.Requests;

public sealed class FakeCreateCustomerRequest : AutoFaker<CreateCustomerRequest>
{
    public FakeCreateCustomerRequest(string? email = null)
    {
        RuleFor(x => x.Email, f => email ?? f.Internet.Email());
    }
}
