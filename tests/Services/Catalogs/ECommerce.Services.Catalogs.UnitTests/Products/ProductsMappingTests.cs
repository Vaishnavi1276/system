using AutoBogus;
using AutoMapper;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;
using ECommerce.Services.Catalogs.UnitTests.Common;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Customers.UnitTests.Products;

public class ProductsMappingTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public ProductsMappingTests(MappingFixture fixture)
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
    public void can_map_create_product_request_create_product()
    {
        var createProductRequest = new AutoFaker<CreateProductRequest>().Generate();
        var res = _mapper.Map<CreateProduct>(createProductRequest);
        res.Name.Should().Be(createProductRequest.Name);
        res.Price.Should().Be(createProductRequest.Price);
    }
}
