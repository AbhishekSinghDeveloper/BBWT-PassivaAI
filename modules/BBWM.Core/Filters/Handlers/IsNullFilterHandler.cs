using BBWM.Core.Filters.TypedFilters;
using System.Linq.Expressions;

namespace BBWM.Core.Filters.Handlers;

internal class IsNullFilterHandler : FilterHandlerBase
{
    private readonly IsNullFilter filter;

    public IsNullFilterHandler(IsNullFilter filter)
    {
        this.filter = filter;
    }

    public override Expression<Func<TEntity, bool>> Handle<TEntity>()
    {
        var item = Expression.Parameter(typeof(TEntity), "item");
        Expression property = GetProperty(item, filter.PropertyName);
        Expression body = null;
        if (property.Type == typeof(string) || !property.Type.IsValueType)
        {
            var targetType = property.Type == typeof(string) ? typeof(string) : typeof(object);

            var constant = Expression.Constant(null, targetType);
            var method = targetType.GetMethod("Equals", new Type[] { targetType, targetType });
            body = Expression.Call(null, method, property, constant);
        }

        if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            body = Expression.Not(Expression.Property(property, "HasValue"));
        }

        if (body is null)
            throw new Exception($"IsNull Filter: {filter.PropertyName} should be DTO, string or Nullable<>. ");

        return Expression.Lambda<Func<TEntity, bool>>(body, item);
    }
}
