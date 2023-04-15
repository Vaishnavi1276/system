using AutoMapper;

namespace ECommerce.Services.Catalogs.UnitTests.Common;

public class MappingFixture
{
    public MappingFixture()
    {
        Mapper = MapperFactory.Create();
    }

    public IMapper Mapper { get; }
}
