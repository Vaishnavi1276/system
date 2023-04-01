using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.TestShared.Fixtures;
using Tests.Shared.Fixtures;

namespace ECommerce.Services.Customers.EndToEndTests;

// https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection, but it can implements multiple ICollectionFixture in its definitions
[CollectionDefinition(Name)]
public class EndToEndTestCollection
    : ICollectionFixture<SharedFixtureWithEfCoreAndMongo<Api.Program, CustomersDbContext, CustomersReadDbContext>>,
        ICollectionFixture<CustomersServiceMockServersFixture>
{
    public const string Name = "End-To-End Test";
}
