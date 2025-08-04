using System.Linq.Expressions;

namespace BBWM.Core.Filters.Handlers;

public class CountableFilterHandler<T> : ValueFilterHandlerBase
{
    private readonly CountableFilterBase<T> _filter;

    protected CountableFilterHandler(CountableFilterBase<T> filter)
    {
        _filter = filter;
    }

    protected virtual Expression GetValue(T value, Type propertyType)
    {
        return Expression.Convert(Expression.Constant(value), propertyType);
    }

    public override Expression<Func<TEntity, bool>> Handle<TEntity>()
    {
        var item = Expression.Parameter(typeof(TEntity), "item");
        var property = GetProperty(item, _filter.PropertyName);
        var value = GetValue(_filter.Value, property.Type);

        Expression body;
        switch (_filter.MatchMode)
        {
            case CountableFilterMatchMode.LessThan:
                body = Expression.LessThan(property, value);
                break;
            case CountableFilterMatchMode.LessThanOrEqual:
                body = Expression.LessThanOrEqual(property, value);
                break;
            case CountableFilterMatchMode.GreaterThanOrEqual:
                body = Expression.GreaterThanOrEqual(property, value);
                break;
            case CountableFilterMatchMode.GreaterThan:
                body = Expression.GreaterThan(property, value);
                break;
            case CountableFilterMatchMode.NotEquals:
                body = Expression.NotEqual(property, value);
                break;
            default:
                body = Expression.Equal(property, value);
                break;
        }

        return Expression.Lambda<Func<TEntity, bool>>(body, item);
    }
}
