using BBWM.Core.Filters.Handlers;

namespace BBWM.Core.Filters.TypedFilters;

[RelatedHandler(typeof(NumberArrayFilterHandler))]
public class NumberArrayFilter : ArrayFilter<int>
{
}
