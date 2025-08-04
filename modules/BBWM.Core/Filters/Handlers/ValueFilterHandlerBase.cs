using System.Linq.Expressions;

namespace BBWM.Core.Filters.Handlers;

public abstract class ValueFilterHandlerBase : FilterHandlerBase
{
    protected override Expression GetProperty(ParameterExpression item, string propertyName)
    {
        Expression property = base.GetProperty(item, propertyName);

        if (!property.Type.IsGenericType) return property;

        return property.Type.GetGenericTypeDefinition() == typeof(Nullable<>) ? Expression.Property(property, "Value") : property;
    }
}
