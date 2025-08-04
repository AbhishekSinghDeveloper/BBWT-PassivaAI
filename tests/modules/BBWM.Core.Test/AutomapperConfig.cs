using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Test;

public static class AutoMapperConfig
{
    public static IMapper CreateMapper(Action<IServiceCollection> configDependencies = default)
    {
        static void config(IMapperConfigurationExpression cfg)
        {
            var bbAssemblies = ModuleLinker.ModuleLinker.GetBbAssemblies();
            cfg.AddMaps(bbAssemblies);
            ProfileBase.CollectAndRegisterMappings(cfg);
            ProfileBase.AutomapEntities(cfg, bbAssemblies);
        }

        if (configDependencies is not null)
        {
            var services = new ServiceCollection();
            configDependencies(services);
            services.AddAutoMapper(config);

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IMapper>();
        }

        return new MapperConfiguration(config).CreateMapper();
    }
}
