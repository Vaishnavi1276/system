using AutoMapper;
using ECommerce.Services.Catalogs.Products;

namespace ECommerce.Services.Catalogs.UnitTests.Common;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappers>();
        });

        return configurationProvider.CreateMapper();
    }
}
