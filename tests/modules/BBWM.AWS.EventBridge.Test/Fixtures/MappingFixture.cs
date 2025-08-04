using AutoMapper;

using BBWM.AWS.EventBridge.Mapping;

namespace BBWM.AWS.EventBridge.Test.Fixtures;

public class MappingFixture
{
    public MappingFixture()
    {
        Mapper = new MapperConfiguration(
            cfg =>
            {
                cfg.AddProfile<AwsEventBridgeJobMappingProfile>();
                cfg.AddProfile<AwsEventBridgeMappingProfile>();
            }).CreateMapper();
    }

    public IMapper Mapper { get; }
}
