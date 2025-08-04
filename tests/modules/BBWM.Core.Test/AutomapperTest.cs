using AutoMapper;
using AutoMapper.Internal;
using BBWM.Core.Test.AutomapModels;

using Bogus;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Test;

public class AutomapperTests
{
    /*[Fact]
    public void AutomapperTest()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            var assemblies = ModuleLinker.ModuleLinker.GetBbAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = Common.GetTypesInheritedFrom<IBaseEntity>(assembly);
                foreach (var type in types)
                {
                    if (type.IsGenericType)
                    {
                        continue;
                    }

                    var rsm = type.GetMethod("RegisterMap", BindingFlags.Static | BindingFlags.Public);
                    if (rsm is null)
                    {
                        continue;
                    }

                    rsm.Invoke(null, new object[] { cfg });
                }
            }

            cfg.AddMaps(assemblies);

            ProfileBase.AutomapEntities(cfg, assemblies);
        });

        configuration.AssertConfigurationIsValid();
    }*/

    [Fact]
    public void Should_Automap_Entity_DTO_BothWays()
    {
        // Arrange
        var mapper = CreateMapper();
        var (entity, dto) = CreateAutomapObjects();

        // Act, Assert
        AssertTypeMaps(mapper);

        var mappedDto = mapper.Map<MyAutomapEntityDTO>(entity);

        Assert.Equal(entity.Id, mappedDto.Id);
        Assert.Equal(entity.ShouldMap1, mappedDto.ShouldMap1);
        Assert.Equal(entity.ShouldMap2, mappedDto.ShouldMap2);

        var mappedEntity = mapper.Map<MyAutomapEntity>(dto);

        Assert.Equal(dto.Id, mappedEntity.Id);
        Assert.Equal(dto.ShouldMap1, mappedEntity.ShouldMap1);
        Assert.Equal(dto.ShouldMap2, mappedEntity.ShouldMap2);
        Assert.Equal(default, mappedEntity.ShouldntMap1);
        Assert.Equal(default, mappedEntity.ShouldntMap2);
    }

    [Fact]
    public void Shouldnt_Automap_Entity_DTO_BothWays()
    {
        // Arrange
        var mapper = CreateMapper(
            cfg =>
            {
                cfg
                    .CreateMap<MyAutomapEntity, MyAutomapEntityDTO>()
                        .ForMember(dto => dto.ShouldMap1, o => o.Ignore())
                    .ReverseMap()
                        .ForMember(e => e.ShouldMap1, o => o.Ignore())
                        .ForMember(e => e.ShouldntMap1, o => o.Ignore())
                        .ForMember(e => e.ShouldntMap2, o => o.Ignore());
            });
        var (entity, dto) = CreateAutomapObjects();

        // Act, Assert
        AssertTypeMaps(mapper);

        var mappedDto = mapper.Map<MyAutomapEntityDTO>(entity);

        Assert.Equal(entity.Id, mappedDto.Id);
        Assert.Equal(default, mappedDto.ShouldMap1);
        Assert.Equal(entity.ShouldMap2, mappedDto.ShouldMap2);

        var mappedEntity = mapper.Map<MyAutomapEntity>(dto);

        Assert.Equal(dto.Id, mappedEntity.Id);
        Assert.Equal(default, mappedEntity.ShouldMap1);
        Assert.Equal(dto.ShouldMap2, mappedEntity.ShouldMap2);
        Assert.Equal(default, mappedEntity.ShouldntMap1);
        Assert.Equal(default, mappedEntity.ShouldntMap2);
    }

    [Fact]
    public void Should_Automap_Entity_DTO()
    {
        // Arrange
        var mapper = CreateMapper(
            cfg =>
            {
                cfg.CreateMap<MyAutomapEntityDTO, MyAutomapEntity>()
                    .ForMember(e => e.ShouldMap1, o => o.Ignore())
                    .ForMember(e => e.ShouldntMap1, o => o.Ignore())
                    .ForMember(e => e.ShouldntMap2, o => o.Ignore());
            });
        var (entity, dto) = CreateAutomapObjects();

        // Act, Assert
        AssertTypeMaps(mapper);

        var mappedDto = mapper.Map<MyAutomapEntityDTO>(entity);

        Assert.Equal(entity.Id, mappedDto.Id);
        Assert.Equal(entity.ShouldMap1, mappedDto.ShouldMap1);
        Assert.Equal(entity.ShouldMap2, mappedDto.ShouldMap2);

        var mappedEntity = mapper.Map<MyAutomapEntity>(dto);

        Assert.Equal(dto.Id, mappedEntity.Id);
        Assert.Equal(default, mappedEntity.ShouldMap1);
        Assert.Equal(dto.ShouldMap2, mappedEntity.ShouldMap2);
        Assert.Equal(default, mappedEntity.ShouldntMap1);
        Assert.Equal(default, mappedEntity.ShouldntMap2);
    }

    [Fact]
    public void Should_Automap_DTO_Entity()
    {
        // Arrange
        var mapper = CreateMapper(
            cfg =>
            {
                cfg.CreateMap<MyAutomapEntity, MyAutomapEntityDTO>()
                    .ForMember(dto => dto.ShouldMap1, o => o.Ignore());
            });
        var (entity, dto) = CreateAutomapObjects();

        // Act, Assert
        AssertTypeMaps(mapper);

        var mappedDto = mapper.Map<MyAutomapEntityDTO>(entity);

        Assert.Equal(entity.Id, mappedDto.Id);
        Assert.Equal(default, mappedDto.ShouldMap1);
        Assert.Equal(entity.ShouldMap2, mappedDto.ShouldMap2);

        var mappedEntity = mapper.Map<MyAutomapEntity>(dto);

        Assert.Equal(dto.Id, mappedEntity.Id);
        Assert.Equal(dto.ShouldMap1, mappedEntity.ShouldMap1);
        Assert.Equal(dto.ShouldMap2, mappedEntity.ShouldMap2);
        Assert.Equal(default, mappedEntity.ShouldntMap1);
        Assert.Equal(default, mappedEntity.ShouldntMap2);
    }

    private static void AssertTypeMaps(IMapper mapper)
    {
        var enityToDto = mapper.ConfigurationProvider.Internal().FindTypeMapFor<MyAutomapEntity, MyAutomapEntityDTO>();
        var dtoToEntity = mapper.ConfigurationProvider.Internal().FindTypeMapFor<MyAutomapEntityDTO, MyAutomapEntity>();

        Assert.NotNull(enityToDto);
        Assert.NotNull(dtoToEntity);
    }

    private static IMapper CreateMapper(Action<IMapperConfigurationExpression> configure = null)
    {
        Action<IMapperConfigurationExpression> configureMapper = cfg =>
        {
            configure?.Invoke(cfg);
            ProfileBase.AutomapEntities(cfg, new[] { Assembly.GetExecutingAssembly() });
        };

        var mapperConfig = new MapperConfiguration(configureMapper);
        mapperConfig.AssertConfigurationIsValid();

        return mapperConfig.CreateMapper();
    }

    private static (MyAutomapEntity, MyAutomapEntityDTO) CreateAutomapObjects()
    {
        var entity = new Faker<MyAutomapEntity>()
                .RuleFor(e => e.Id, f => f.Random.Number(max: 1000))
                .RuleFor(e => e.ShouldMap1, f => f.Random.String2(5))
                .RuleFor(e => e.ShouldMap2, f => f.Random.Number(6))
                .RuleFor(e => e.ShouldntMap1, f => f.Random.String2(4))
                .RuleFor(e => e.ShouldntMap2, f => f.Random.Number(7))
                .Generate();

        var dto = new Faker<MyAutomapEntityDTO>()
            .RuleFor(d => d.Id, f => f.Random.Number(max: 1000))
            .RuleFor(e => e.ShouldMap1, f => f.Random.String2(5))
            .RuleFor(e => e.ShouldMap2, f => f.Random.Number(6))
            .Generate();

        return (entity, dto);
    }
}
