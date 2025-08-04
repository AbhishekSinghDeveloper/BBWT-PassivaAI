using AutoMapper;
using AutoMapper.Configuration;
using AutoMapper.Internal;
using BBWM.Core.Data;
using BBWM.Core.Utils;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace BBWM.Core;

/// <summary>
/// Inherit all automapper profiles from this. This will allow you to avoid errors when you forget to ignore some compound property and waste
/// time trying to understand what is wrong. Compound properties should always be ignored when mapping dto to entities and preparing database
/// save. Those properties should be saved explicitly - as separate entity or collection.
/// In real projects the amount of such properties can be tens and hundreds (see Charles Trent). And maintaining this manually is very error
/// prone and time consuming. Also bear in mind that automapper mappings is one of the most volatile areas of code.
/// </summary>
/// <remarks>
/// Note: The rules below get most likely applied even if you don't inherit from the ProfileBase. See Startup.cs.
/// </remarks>
public class ProfileBase : Profile
{
    public ProfileBase()
    {
        ApplyRules(this);
    }

    public static void ApplyRules(IProfileExpression cfg)
    {
        /* Here we Ignore all compound child property mappings in Dto -> Entity direction.
         * It means that they should be saved to database explicitly. As it has always been in T2 projects.
         */
        cfg.Internal().ForAllMaps((typeMap, me) =>
        {
            MappingExpression m = me as MappingExpression;
            if (!typeof(IBaseEntity).IsAssignableFrom(m.DestinationType))
            {
                return;
            }

            // Exclude potentially too tricky cases in BBWM modules.
            var assembly = m.DestinationType.Assembly;
            if (ModuleLinker.ModuleLinker.IsModuleAssembly(assembly)
                && !ModuleLinker.ModuleLinker.IsDemoModuleAssembly(assembly))
            {
                return;
            }

            me.ForAllMembers(i =>
            {
                var pi = i.DestinationMember as System.Reflection.PropertyInfo;
                if (pi.PropertyType == typeof(string))
                    return;

                if (typeof(IBaseEntity).IsAssignableFrom(pi.PropertyType))
                {
                    if (!Attribute.IsDefined(pi, typeof(DoNotAutoignoreAttribute)))
                    {
                        i.Ignore();
                    }
                    /* If we have child entity then it is better to add correspoding Id at once.
                     * This is not very important rule. Feel free to disable it if it doesn't suit your project.
                     */
                    if (!Attribute.IsDefined(pi, typeof(DoNotRequireChildIdAttribute)))
                    {
                        var childIdName = pi.Name + "Id";
                        var childIdProperty = pi.DeclaringType.GetProperty(childIdName);
                        if (childIdProperty is null)
                        {
                            throw new Exception(
                                $"Error while mapping {pi.DeclaringType.Name}::{pi.Name}. If you have child Entity with " +
                                $"name like 'Child' then please add corresponding Id property with name like 'ChildId'");
                        }
                    }
                }
                if (typeof(IEnumerable).IsAssignableFrom(pi.PropertyType))
                {

                    i.Ignore();
                }
            });
        });

        // Detects duplicate mappings
        DetectDuplicateMappings(cfg);
    }

    /// <summary>
    /// Detects duplicate mappings. They can be quite nasty if they are located in different files and are actully different.
    /// </summary>
    /// <param name="cfg"></param>
    private static void DetectDuplicateMappings(IProfileExpression cfg)
    {
        var alreadyMapped = new List<Tuple<Type, Type>>();
        cfg.Internal().ForAllMaps((typeMap, me) =>
        {
            MappingExpression m = me as MappingExpression;
            var from = m.SourceType;
            var to = m.DestinationType;
            var mapping = alreadyMapped.SingleOrDefault(i => i.Item1 == from && i.Item2 == to);
            if (mapping is not null)
            {
                throw new Exception($"Error while mapping {from.Name} to {to.Name}. Mapping already exists. Please remove duplicate to avoid nondeterministic behaviour.");
            }
            alreadyMapped.Add(new Tuple<Type, Type>(from, to));
        });
    }

    /// <summary>
    /// BBWT3 has a functional which automatically collects all existent static and public implementations of
    /// <c>RegisterMap()</c> method through all the project's models inherited from <see cref="IEntity"/>. The method is supposed
    /// to contain code to register mappings.
    /// </summary>
    /// <remarks>
    /// This is a recommended approach when you register mapping manually. It was developed in BBWT for ease of use.
    /// Doing so you keep a model along with its mapping and avoid creating an extra file with a mapping class, at
    /// least you avoid adding of extra code lines with mapping into a separate file.
    /// <para>
    /// <see href="https://wiki.bbconsult.co.uk/display/BLUEB/BBWT3+Explained+to+Beginners#BBWT3ExplainedtoBeginners-Option2.AddRegisterMap()methodintothemodel'sclass">
    /// See Wiki page</see>
    /// </para>
    /// </remarks>
    /// <param name="cfg">Mapper configuration.</param>
    public static void CollectAndRegisterMappings(IMapperConfigurationExpression cfg)
    {
        ApplyRules(cfg);

        var assemblies = ModuleLinker.ModuleLinker.GetBbAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = Common.GetTypesInheritedFrom<IBaseEntity>(assembly);
            foreach (Type type in types)
            {
                if (type.IsGenericType) continue;
                MethodInfo rsm = type.GetMethod("RegisterMap", BindingFlags.Static | BindingFlags.Public);
                if (rsm is null) continue;

                rsm.Invoke(null, new object[] { cfg });
            }
        }
    }

    /// <summary>
    /// Automatically adds default mapping of entities to DTO and reverse.
    /// Entitiy to DTO matching is based on string template: <b>[entity_name] matches [entity_name]DTO.</b>
    /// </summary>
    /// <remarks>
    /// <para>This option helps to avoid creating mapping profiles manually when mapping doesn't need customization.
    /// The options is supposed to save time for developer, because the project's code may have many default DTO mappings.
    /// As result of this optimization, developer gets free from adding default mappings like this:
    /// <code>
    /// cfg.CreateMap&lt;Order, OrderDTO&gt;().ReverseMap();
    /// </code>
    /// </para>
    /// <para>
    /// As the automapping logic works based on a naming policy we restrict the automapping of entities and DTOs only 
    /// within the same assembly to avoid name collisions and hence a (potentially) wrong automapping configuration. 
    /// If you have more than one entity with the same name in one of your assemblies, make sure to create the mappings 
    /// of those entities yourself.
    /// </para>
    /// <para>
    /// <see href="https://wiki.bbconsult.co.uk/display/BLUEB/BBWT3+Explained+to+Beginners#BBWT3ExplainedtoBeginners-Option1.Automaticmappingsregistrationbyclassnames">
    /// See Wiki page</see>
    /// </para>
    /// </remarks>        
    /// <param name="cfg">Mapper configuration.</param>
    /// <param name="assemblies">A list of included assemblies.</param>
    public static void AutomapEntities(IMapperConfigurationExpression cfg, IEnumerable<Assembly> assemblies)
    {
        assemblies = assemblies.ToList();
        var dtos = assemblies
            .SelectMany(
                assembly => assembly
                    .GetTypes()
                    .Where(dto => dto.Name.EndsWith("DTO", true, CultureInfo.InvariantCulture) &&
                                  dto.IsClass &&
                                  !dto.IsAbstract &&
                                  dto.IsPublic))
            .ToArray();
        var entities = assemblies
            .SelectMany(Common.GetTypesInheritedFrom<IBaseEntity>)
            .Where(entity => entity.IsClass && !entity.IsAbstract && entity.IsPublic);

        var currentMappings = LoadCurrentMappings(cfg);

        foreach (var entityType in entities)
        {
            var dtoType = dtos.FirstOrDefault(
                dto => string.Compare(dto.Name, $"{entityType.Name}DTO", true, CultureInfo.InvariantCulture) == 0 &&
                       dto.Assembly == entityType.Assembly);
            if (dtoType != default)
            {
                foreach (var (source, destination) in new[] { (entityType, dtoType), (dtoType, entityType) })
                {
                    if (!currentMappings.Contains((source, destination)))
                    {
                        cfg.CreateMap(source, destination, MemberList.None);
                    }
                }
            }
        }
    }

    private static HashSet<(Type, Type)> LoadCurrentMappings(IMapperConfigurationExpression cfg)
    {
        var typeMapConfigs = new List<TypeMapConfiguration>();

        if (cfg is IProfileConfiguration profileConfig)
        { typeMapConfigs.AddRange(profileConfig.TypeMapConfigs); }

        if (cfg is MapperConfigurationExpression mapperConfig)
        {
            var profilesTypeMapConfigs = mapperConfig.Internal().Profiles.SelectMany(profile => profile.TypeMapConfigs);
            typeMapConfigs.AddRange(profilesTypeMapConfigs);
        }

        var reverseTypeMapConfigs = typeMapConfigs
            .Select(typeMapConfig => typeMapConfig.ReverseTypeMap)
            .Where(typeMapConfig => typeMapConfig != default)
            .ToList();
        typeMapConfigs.AddRange(reverseTypeMapConfigs);

        return typeMapConfigs
            .Select(typeMapConfig => (typeMapConfig.SourceType, typeMapConfig.DestinationType))
            .ToHashSet();
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class DoNotRequireChildIdAttribute : Attribute
{

}

/// <summary>
/// By default compound properties are ignored (excluded from mapping) automatically by ProfileBase.
/// In some cases for you own risk you can use this attribute to skip auto-ignore option and handle it manually. 
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DoNotAutoignoreAttribute : Attribute
{

}
