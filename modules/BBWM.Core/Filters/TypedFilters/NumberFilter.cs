using BBWM.Core.Filters.Handlers;

namespace BBWM.Core.Filters.TypedFilters;

[RelatedHandler(typeof(NumberFilterHandler))]
public class NumberFilter : CountableFilterBase<double>
{
}

[RelatedHandler(typeof(CountableBetweenFilterHandler<double>))]
public class NumberBetweenFilter : CountableBetweenFilterBase<double>
{
}
