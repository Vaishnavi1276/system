using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using FluentAssertions;
using Humanizer;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests.Customers.Features.GettingCustomerByCustomerId.v1;

public class GetCustomerByCustomerIdTests : CustomerServiceIntegrationTestBase
{
    public GetCustomerByCustomerIdTests(
        SharedFixtureWithEfCoreAndMongo<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper) { }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    internal async Task can_returns_valid_read_customer_model()
    {
        // Arrange
        Customer fakeCustomer = new FakeCustomerReadModel().Generate();
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomer);

        // Act
        var query = new GetCustomerByCustomerId(fakeCustomer.CustomerId);
        var customer = (await SharedFixture.SendAsync(query)).Customer;

        // Assert
        customer.Should().BeEquivalentTo(fakeCustomer, options => options.ExcludingMissingMembers());
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    internal async Task must_throw_not_found_exception_when_item_does_not_exists_in_mongodb()
    {
        // Act
        var query = new GetCustomerByCustomerId(100);
        Func<Task> act = async () => _ = await SharedFixture.SendAsync(query);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerNotFoundException>();
    }
}
