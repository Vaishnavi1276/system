using System.Linq.Expressions;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Customers.Features.GettingCustomerById.v1;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using ECommerce.Services.Customers.UnitTests.Common;
using FluentAssertions;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Customers.UnitTests.Customers.Features.GettingCustomerById.v1;

public class GetCustomerByIdTests : CustomerServiceUnitTestBase
{
    private readonly ICustomersReadUnitOfWork _customersReadUnitOfWork;

    public GetCustomerByIdTests()
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
        var query = new GetCustomerById(customerReadModel.Id);
        var handler = new GetCustomerByIdHandler(_customersReadUnitOfWork, Mapper);
        var res = await handler.Handle(query, CancellationToken.None);

        await _customersReadUnitOfWork.CustomersRepository
            .Received(1)
            .FindOneAsync(
                Arg.Is<Expression<Func<Customer, bool>>>(exp => exp.Compile()(customerReadModel) == true),
                Arg.Any<CancellationToken>()
            );
        res.Should().NotBeNull();
        res.Customer.Id.Should().Be(customerReadModel.Id);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var query = new GetCustomerById(invalidId);
        var handler = new GetCustomerByIdHandler(_customersReadUnitOfWork, Mapper);

        // Act
        Func<Task> act = async () => _ = await handler.Handle(query, CancellationToken.None);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerNotFoundException>();

        await _customersReadUnitOfWork.CustomersRepository
            .Received(1)
            .FindOneAsync(
                Arg.Is<Expression<Func<Customer, bool>>>(exp => exp.Compile()(new Customer { Id = invalidId }) == true),
                Arg.Any<CancellationToken>()
            );
    }
}
