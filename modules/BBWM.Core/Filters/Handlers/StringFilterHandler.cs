using BBWM.Core.Filters.TypedFilters;
using System.Linq.Expressions;

namespace BBWM.Core.Filters.Handlers;

public class StringFilterHandler : FilterHandlerBase
{
    private readonly StringFilter _filter;

    public StringFilterHandler(StringFilter filter)
    {
        _filter = filter;
    }

    public override Expression<Func<TEntity, bool>> Handle<TEntity>()
    {
        var item = Expression.Parameter(typeof(TEntity), "item");
        var property = Expression.Call(GetProperty(item, _filter.PropertyName), "ToUpper", null);
        var value = Expression.Constant(_filter.Value?.ToUpperInvariant());

        // We have made the decision that BBWT3 will, by default, convert all searches to be case-insensitive.
        // We are not currently supporting the option for case-sensitive searches, but that feature could be added at a later point.
        string methodName;
        var isNotNullExpression = Expression.NotEqual(property, Expression.Constant(null));
        methodName = _filter.MatchMode switch
        {
            StringFilterMatchMode.Contains => "Contains",
            StringFilterMatchMode.NotContains => "Contains",
            StringFilterMatchMode.StartsWith => "StartsWith",
            StringFilterMatchMode.EndsWith => "EndsWith",
            _ => "Equals"
        };
        var method = typeof(string).GetMethod(methodName, new Type[] { typeof(string) });
        var body = Expression.Call(property, method, value);
        var resultExpression = Expression.AndAlso(
            isNotNullExpression,
            _filter.MatchMode != StringFilterMatchMode.NotContains && _filter.MatchMode != StringFilterMatchMode.NotEquals
                ? body
                : Expression.Not(body));
        return Expression.Lambda<Func<TEntity, bool>>(resultExpression, item);
    }

}
