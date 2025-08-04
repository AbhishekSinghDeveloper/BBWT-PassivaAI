using BBWM.Core.Filters.TypedFilters;
using System.Linq.Expressions;

namespace BBWM.Core.Filters.Handlers;

public class DateFilterHandler : CountableFilterHandler<DateTime>
{
    private readonly DateFilter _filter;


    public DateFilterHandler(DateFilter filter) : base(filter) => _filter = filter;


    protected override Expression GetValue(DateTime value, Type propertyType)
    {
        // Fix for filtering on DateTimeOffset fields. See https://docs.microsoft.com/en-us/dotnet/standard/datetime/converting-between-datetime-and-offset for more details.
        return Expression.Convert(Expression.Constant(DateTime.SpecifyKind(value, DateTimeKind.Utc)), propertyType);
    }

    public override Expression<Func<TEntity, bool>> Handle<TEntity>()
    {
        /*
         * By default, an "Equals" match mode means that the value matches accurate to a day,
         * and the time value may seem redundant. But this does not work correctly given time zone offsets.
         * Therefore, we have to present the "day" as the time interval between specified value and plus 24 hours,
         * but not just cut the time part.
         */
        if (_filter.MatchMode == CountableFilterMatchMode.Equals || _filter.MatchMode == CountableFilterMatchMode.NotEquals)
        {
            var item = Expression.Parameter(typeof(TEntity), "item");
            var property = GetProperty(item, _filter.PropertyName);

            var dayFromValue = Expression.Convert(Expression.Constant(_filter.Value.ToUniversalTime()), property.Type);
            var dayToValue = Expression.Convert(Expression.Constant(_filter.Value.AddHours(24).ToUniversalTime()), property.Type);

            var expr1 = _filter.MatchMode == CountableFilterMatchMode.Equals
                ? Expression.GreaterThanOrEqual(property, dayFromValue)
                : Expression.LessThan(property, dayFromValue);
            var expr2 = _filter.MatchMode == CountableFilterMatchMode.Equals
                ? Expression.LessThan(property, dayToValue)
                : Expression.GreaterThanOrEqual(property, dayToValue);

            var body = _filter.MatchMode == CountableFilterMatchMode.Equals
                ? Expression.And(expr1, expr2)
                : Expression.Or(expr1, expr2);

            return Expression.Lambda<Func<TEntity, bool>>(body, item);
        }

        return base.Handle<TEntity>();
    }
}
