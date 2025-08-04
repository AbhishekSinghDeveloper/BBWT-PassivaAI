using System.Linq.Expressions;

namespace BBWM.Core.Filters.Handlers;

public abstract class FilterHandlerBase
{
    public abstract Expression<Func<TEntity, bool>> Handle<TEntity>();

    protected virtual Expression GetProperty(ParameterExpression item, string propertyName)
    {
        var parts = propertyName.Split('.');
        var property = parts.Aggregate<string, Expression>(item, Expression.Property);
        return property;
    }
}
