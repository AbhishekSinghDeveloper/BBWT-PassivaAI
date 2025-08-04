using AutoMapper;
using AutoMapper.Internal;

using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BBWM.Core.ModelHashing;

public class ModelHashingService : IModelHashingService
{
    private readonly List<Type> IgnoreHashingModelsList = new();

    private readonly Dictionary<Type, Dictionary<MemberInfo, Type>> ManualHashingPropertiesDict = new();

    private readonly Dictionary<Type, List<MemberInfo>> IgnoreHashingPropertiesDict = new();

    private readonly List<KeysMap> KeyMapItems = new();

    public KeysMap[] GetMaps(Type modelType)
    {
        return GetKeysMaps()
            .Concat(GetManualHashingProperties())
            .GroupBy(m => new { m.ModelType, m.Property })
            .Select(g => g.FirstOrDefault())
            .Where(m => m.ModelType.FullName == modelType.FullName)
            .ToArray();
    }

    private IEnumerable<KeysMap> GetKeysMaps()
    {
        return KeyMapItems
            .Where(m => IgnoreHashingModelsList.All(t => t != m.ModelType)
                && (IgnoreHashingPropertiesDict.ContainsKey(m.ModelType) ?
                    IgnoreHashingPropertiesDict[m.ModelType] :
                    new List<MemberInfo>()).All(p => !p.Name.Equals(m.Property, StringComparison.InvariantCultureIgnoreCase)))
            .Select(m => new KeysMap
            {
                ModelType = m.ModelType,
                Property = m.Property,
                Salt = m.Salt
            });
    }

    private IEnumerable<KeysMap> GetManualHashingProperties()
    {
        return ManualHashingPropertiesDict.SelectMany(pair => pair.Value.Select(property => new KeysMap
        {
            ModelType = pair.Key,
            Property = ToLowerCase(property.Key.Name),
            Salt = HashingHelper.GetTypeHash(property.Value)
        }));
    }

    public void Register(IMapper mapper, IDbContext context)
    {
        foreach (var typeMaps in mapper.ConfigurationProvider.Internal().GetAllTypeMaps())
        {
            var keyProperties = ((DbContext)context).FindKeys(typeMaps.SourceType);
            if (keyProperties is not null && keyProperties.Any())
            {
                KeyMapItems.AddRange(typeMaps.PropertyMaps
                    .Where(m => m.CustomMapExpression is null && !m.Ignored &&
                        (m.SourceType == typeof(int) && m.DestinationType == typeof(int) ||
                        m.SourceType == typeof(int?) && m.DestinationType == typeof(int?)))
                    .Join(keyProperties.ToList(), x => x.SourceMember.Name, x => x.Key.Name, (m, k) => new KeysMap
                    {
                        ModelType = typeMaps.DestinationType,
                        Property = ToLowerCase(m.DestinationName),
                        Salt = HashingHelper.GetTypeHash(k.Value)
                    }));
            }
        }
    }

    public int? UnHashProperty(Type dtoType, string propertyName, string propertyValue)
    {
        var map = GetMaps(dtoType).SingleOrDefault(keyMapItem => keyMapItem.Property.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        return map is null ? null : HashingHelper.GetKeyFromHashString(propertyValue, map.Salt);
    }

    public string HashProperty(Type dtoType, string propertyName, int propertyValue)
    {
        var map = GetMaps(dtoType).SingleOrDefault(keyMapItem => keyMapItem.Property.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        return map is null ? null : HashingHelper.AppendHashToKey(propertyValue, map.Salt);
    }

    public void ManualPropertyHashing(Type modelType, Type entityType, MemberInfo memberInfo)
    {
        if (!ManualHashingPropertiesDict.ContainsKey(modelType))
        {
            ManualHashingPropertiesDict.Add(modelType, new Dictionary<MemberInfo, Type>());
        }

        ManualHashingPropertiesDict[modelType].Add(memberInfo, entityType);
    }

    public void IgnorePropertiesHashing(Type modelType, IEnumerable<MemberInfo> members)
    {
        if (!IgnoreHashingPropertiesDict.ContainsKey(modelType))
        {
            IgnoreHashingPropertiesDict.Add(modelType, new List<MemberInfo>());
        }

        foreach (var memberInfo in members)
        {
            IgnoreHashingPropertiesDict[modelType].Add(memberInfo);
        }
    }

    public void IgnoreModelHashing(Type type)
    {
        IgnoreHashingModelsList.Add(type);
    }

    static string ToLowerCase(string name)
        => char.ToLowerInvariant(name[0]) + name.Substring(1);
}
