using BBWM.Core.Filters.Handlers;

namespace BBWM.Core.Filters.TypedFilters;

[RelatedHandler(typeof(DateFilterHandler))]
public class DateFilter : CountableFilterBase<DateTime>
{
}

[RelatedHandler(typeof(CountableBetweenFilterHandler<DateTime>))]
public class DateBetweenFilter : CountableBetweenFilterBase<DateTime>
{
}
