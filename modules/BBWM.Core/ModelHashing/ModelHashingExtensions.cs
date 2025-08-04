using System.Linq.Expressions;
using System.Reflection;

namespace BBWM.Core.ModelHashing;

public static class ModelServiceExtensions
{
    public static void ManualPropertyHashing<TDTO, TEntity>(this IModelHashingService modelHashingService, Expression<Func<TDTO, int?>> property)
    {
        var modelType = typeof(TDTO);
        Type entityType = typeof(TEntity);
        modelHashingService.ManualPropertyHashing(modelType, entityType, property.GetPropertyInfo());
    }

    public static void IgnorePropertiesHashing<TDTO>(this IModelHashingService modelHashingService, params Expression<Func<TDTO, int>>[] properties)
    {
        var modelType = typeof(TDTO);
        IEnumerable<MemberInfo> members = properties.Select(property => property.GetPropertyInfo());
        modelHashingService.IgnorePropertiesHashing(modelType, members);
    }

    public static IModelHashingService IgnoreModelHashing<TDTO>(this IModelHashingService modelHashingService)
    {
        modelHashingService.IgnoreModelHashing(typeof(TDTO));
        return modelHashingService;
    }

    public static int? UnHashProperty<TDTO>(this IModelHashingService modelHashingService, string propertyName, string propertyValue)
    {
        var entityType = typeof(TDTO);
        return modelHashingService.UnHashProperty(entityType, propertyName, propertyValue);
    }

    public static string HashProperty<TDTO>(this IModelHashingService modelHashingService, TDTO dto, string propertyName)
    {
        var entityType = typeof(TDTO);
        return entityType.GetProperty(propertyName)?.GetValue(dto) is int intPropertyValue
            ? modelHashingService.HashProperty<TDTO>(propertyName, intPropertyValue)
            : null;
    }

    public static string HashProperty<TDTO>(this IModelHashingService modelHashingService, string propertyName, int propertyValue)
    {
        var entityType = typeof(TDTO);
        return modelHashingService.HashProperty(entityType, propertyName, propertyValue);
    }


    private static MemberInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
    {
        var temp = propertyLambda.Body;
        while (temp is UnaryExpression)
        {
            temp = (temp as UnaryExpression).Operand;
        }
        var member = temp as MemberExpression;
        return member.Member;
    }
}
