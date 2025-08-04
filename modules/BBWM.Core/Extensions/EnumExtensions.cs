using System.Reflection;
using System.Runtime.Serialization;

namespace BBWM.Core.Extensions;

public static class EnumExtensions
{
    public static Dictionary<string, object> GetEnumNamesValues(this Type enumType)
    {
        if (!enumType.IsEnum) throw new ArgumentException("The specified type is not an Enum.");

        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType);

        var result = new Dictionary<string, object>();
        for (var index = 0; index < names.Length; index++)
        {
            var memberInfo = enumType.GetMember(names[index])[0];
            var enumMemberAttribute =
                memberInfo.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();

            result.Add(enumMemberAttribute is not null ? enumMemberAttribute.Value : names[index], values.GetValue(index));
        }

        return result;
    }

    public static String ToEnumValueString<T>(this T value)
        where T : struct, IConvertible
    {
        return typeof(T)
            .GetTypeInfo()
            .DeclaredMembers
            .SingleOrDefault(x => x.Name == value.ToString())
            ?.GetCustomAttribute<EnumMemberAttribute>(false)
            ?.Value;
    }
}
