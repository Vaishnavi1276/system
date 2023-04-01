using AutoBogus;
using AutoMapper;
using ECommerce.Services.Customers.Customers.Dtos.v1;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;
using ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.Read.Mongo;
using ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using ECommerce.Services.Customers.UnitTests.Common;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Customers.UnitTests.Customers;

public class CustomersMappingTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public CustomersMappingTests(MappingFixture fixture)
    {
        _mapper = fixture.Mapper;
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_success_with_valid_configuration()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_read_model_to_customer_read_dto()
    {
        var customerReadModel = AutoFaker.Generate<Customer>();
        var res = _mapper.Map<CustomerReadDto>(customerReadModel);
        customerReadModel.CustomerId.Should().Be(res.CustomerId);
        customerReadModel.FullName.Should().Be(res.Name);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_to_create_mongo_customer_read_models()
    {
        var customer = new FakeCustomer().Generate();
        var res = _mapper.Map<CreateCustomerRead>(customer);
        customer.Id.Value.Should().Be(res.CustomerId);
        customer.Name.FullName.Should().Be(res.FullName);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_create_mongo_customer_read_models_to_customer_read_model()
    {
        var createReadCustomer = AutoFaker.Generate<CreateCustomerRead>();
        var res = _mapper.Map<Customer>(createReadCustomer);
        createReadCustomer.Id.Should().Be(res.Id);
        createReadCustomer.CustomerId.Should().Be(res.CustomerId);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_to_update_mongo_customer_read_model()
    {
        var customer = new FakeCustomer().Generate();
        var res = _mapper.Map<UpdateCustomerRead>(customer);
        customer.Id.Value.Should().Be(res.CustomerId);
        customer.Name.FullName.Should().Be(res.FullName);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_update_mongo_customer_reads_model_to_customer_read_model()
    {
        var updateMongoCustomerReadsModel = AutoFaker.Generate<UpdateCustomerRead>();
        var res = _mapper.Map<Customer>(updateMongoCustomerReadsModel);
        updateMongoCustomerReadsModel.Id.Should().Be(res.Id);
        updateMongoCustomerReadsModel.CustomerId.Should().Be(res.CustomerId);
    }
}
