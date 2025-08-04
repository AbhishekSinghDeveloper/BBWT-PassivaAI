using BBWM.Core.Filters.TypedFilters;
using System.Linq.Expressions;

namespace BBWM.Core.Filters.Handlers;

public class BooleanFilterHandler : FilterHandlerBase
{
    private readonly BooleanFilter filter;

    public BooleanFilterHandler(BooleanFilter filter)
    {
        this.filter = filter;
    }

    protected virtual Expression GetValue(bool value, Type propertyType)
    {
        return Expression.Convert(Expression.Constant(value), propertyType);
    }

    public override Expression<Func<TEntity, bool>> Handle<TEntity>()
    {
        var item = Expression.Parameter(typeof(TEntity), "item");

        if (filter.Value.HasValue)
        {
            var property = GetProperty(item, filter.PropertyName);
            var value = GetValue(filter.Value.Value, property.Type);
            var body = Expression.Equal(property, value);
            return Expression.Lambda<Func<TEntity, bool>>(body, item);
        }

        return Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), item);
    }
}
