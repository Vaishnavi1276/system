using System.Linq.Expressions;
using Bogus;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using ECommerce.Services.Customers.UnitTests.Common;
using FluentAssertions;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Customers.UnitTests.Customers.Features.GettingCustomerByCustomerId;

public class GetCustomerByCustomerIdTests : CustomerServiceUnitTestBase
{
    private readonly ICustomersReadUnitOfWork _customersReadUnitOfWork;

    public GetCustomerByCustomerIdTests()
    {
        _customersReadUnitOfWork = Substitute.For<ICustomersReadUnitOfWork>();
        var customersReadRepository = Substitute.For<ICustomerReadRepository>();
        _customersReadUnitOfWork.CustomersRepository.Returns(customersReadRepository);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_get_existing_customer_with_valid_input()
    {
        // Arrange
        var customerReadModel = new FakeCustomerReadModel().Generate();
        _customersReadUnitOfWork.CustomersRepository
            .FindOneAsync(
                Arg.Is<Expression<Func<Customer, bool>>>(exp => exp.Compile()(customerReadModel) == true),
                Arg.Any<CancellationToken>()
            )
            .Returns(customerReadModel);

        // Act
        var query = new GetCustomerByCustomerId(customerReadModel.CustomerId);
        var handler = new GetCustomerByCustomerIdHandler(_customersReadUnitOfWork, Mapper);
        var res = await handler.Handle(query, CancellationToken.None);

        await _customersReadUnitOfWork.CustomersRepository
            .Received(1)
            .FindOneAsync(
                Arg.Is<Expression<Func<Customer, bool>>>(exp => exp.Compile()(customerReadModel) == true),
                Arg.Any<CancellationToken>()
            );
        res.Should().NotBeNull();
        res.Customer.CustomerId.Should().Be(customerReadModel.CustomerId);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var invalidId = new Faker().Random.Number(1, 100);
        var query = new GetCustomerByCustomerId(invalidId);
        var handler = new GetCustomerByCustomerIdHandler(_customersReadUnitOfWork, Mapper);

        // Act
        Func<Task> act = async () => _ = await handler.Handle(query, CancellationToken.None);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerNotFoundException>();

        await _customersReadUnitOfWork.CustomersRepository
            .Received(1)
            .FindOneAsync(
                Arg.Is<Expression<Func<Customer, bool>>>(
                    exp => exp.Compile()(new Customer { CustomerId = invalidId }) == true
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
