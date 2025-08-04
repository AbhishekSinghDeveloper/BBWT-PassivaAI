using System.Reflection;

namespace BBWM.Core.Utils;

public static class Common
{
    public static IEnumerable<Type> GetTypesWithAttribute<TAttr>(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(TAttr), true).Length > 0)
            {
                yield return type;
            }
        }
    }

    public static IEnumerable<Type> GetTypesInheritedFrom<TParent>(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(TParent).IsAssignableFrom(type))
            {
                yield return type;
            }
        }
    }

    public static IEnumerable<Type> GetTypesStrictlyInheritedFrom<TParent>(Assembly assembly)
        => GetTypesInheritedFrom<TParent>(assembly).Where(t => t != typeof(TParent));
}
