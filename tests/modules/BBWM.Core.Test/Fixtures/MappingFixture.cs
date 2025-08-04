using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Test.Fixtures;

public class MappingFixture
{
    private IMapper _simpleMapper = null;

    public IMapper DefaultMapper { get => _simpleMapper ??= GetMapper(null); }

    public IMapper GetMapper(Action<IServiceCollection> configDependencies) => AutoMapperConfig.CreateMapper(configDependencies);
}
