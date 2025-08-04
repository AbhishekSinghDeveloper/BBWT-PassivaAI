namespace BBWM.Core.Extensions;

public static class TypeExtensions
{
    public static bool IsSubClassOfGeneric(this Type type, Type genericType)
    {
        if (!genericType.IsGenericType)
            throw new Exception("The variable being compared to is not an instance of a generic class.");

        while (type is not null)
        {
            var currentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (currentType == genericType) return true;
            type = type.BaseType;
        }

        return false;
    }
}
