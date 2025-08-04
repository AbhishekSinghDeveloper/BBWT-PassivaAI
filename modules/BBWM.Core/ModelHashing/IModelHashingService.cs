using AutoMapper;

using BBWM.Core.Data;

using System.Reflection;

namespace BBWM.Core.ModelHashing;

public interface IModelHashingService
{
    KeysMap[] GetMaps(Type modelType);
    void Register(IMapper mapper, IDbContext context);

    void ManualPropertyHashing(Type modelType, Type entityType, MemberInfo memberInfo);
    void IgnorePropertiesHashing(Type modelType, IEnumerable<MemberInfo> members);
    void IgnoreModelHashing(Type type);

    int? UnHashProperty(Type dtoType, string propertyName, string propertyValue);
    string HashProperty(Type dtoType, string propertyName, int propertyValue);
}
