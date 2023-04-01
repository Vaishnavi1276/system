using BuildingBlocks.Validation;
using ECommerce.Services.Customers.Api;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tests.Shared.Extensions;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;
using Guid = System.Guid;

namespace ECommerce.Services.Customers.EndToEndTests.Customers.Features.GettingCustomerById.v1;

public class GetCustomerByIdTests : CustomerServiceEndToEndTestBase
{
    public GetCustomerByIdTests(
        SharedFixtureWithEfCoreAndMongo<Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper) { }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task can_returns_ok_status_code_using_valid_id_and_auth_credentials()
    {
        // Arrange
        var fakeCustomer = new FakeCustomerReadModel().Generate();
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomer);

        var route = Constants.Routes.Customers.GetById(fakeCustomer.Id);

        // Act
        var response = await SharedFixture.NormalUserHttpClient.GetAsync(route);

        // Assert
        response.Should().Be200Ok();
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task can_returns_valid_response_using_valid_id_and_auth_credentials()
    {
        // Arrange
        var fakeCustomer = new FakeCustomerReadModel().Generate();
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomer);

        var route = Constants.Routes.Customers.GetById(fakeCustomer.Id);

        // Act
        var response = await SharedFixture.NormalUserHttpClient.GetAsync(route);

        // Assert
        response
            .Should()
            .BeAs(
                new
                {
                    Customer = new
                    {
                        Id = fakeCustomer.Id,
                        CustomerId = fakeCustomer.CustomerId,
                        IdentityId = fakeCustomer.IdentityId
                    }
                }
            );

        // // OR
        //  response
        //      .Should()
        //      .Satisfy(
        //          givenModelStructure: new
        //          {
        //              Customer = new
        //              {
        //                  Id = default(Guid),
        //                  CustomerId = default(long),
        //                  IdentityId = default(Guid)
        //              }
        //          },
        //          assertion: model =>
        //          {
        //              model.Customer.CustomerId.Should().Be(fakeCustomer.CustomerId);
        //              model.Customer.Id.Should().Be(fakeCustomer.Id);
        //              model.Customer.IdentityId.Should().Be(fakeCustomer.IdentityId);
        //          }
        //      );
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task must_returns_not_found_status_code_when_customer_not_exists()
    {
        // Arrange
        var notExistsId = Guid.NewGuid();
        var route = Constants.Routes.Customers.GetById(notExistsId);

        // Act
        var response = await SharedFixture.AdminHttpClient.GetAsync(route);

        // Assert
        response
            .Should()
            .HaveError("title", nameof(CustomerNotFoundException))
            .And.HaveError("type", "https://somedomain/not-found-error")
            .And.HaveErrorMessage($"Customer with id '{notExistsId}' not found.")
            .And.Be404NotFound();
    }

    [Fact]
    [CategoryTrait(TestCategory.EndToEnd)]
    public async Task must_returns_bad_request_status_code_with_invalid()
    {
        // Arrange
        var invalidId = Guid.Empty;
        var route = Constants.Routes.Customers.GetById(invalidId);

        // Act
        var response = await SharedFixture.AdminHttpClient.GetAsync(route);

        // Assert
        response
            .Should()
            .ContainsProblemDetail(
                new ProblemDetails
                {
                    Detail = "'Id' must not be empty.",
                    Title = nameof(ValidationException),
                    Type = "https://somedomain/input-validation-rules-error",
                }
            )
            .And.Be400BadRequest();
    }
}
