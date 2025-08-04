using BBWM.Core.Filters.Handlers;

namespace BBWM.Core.Filters.TypedFilters;

[RelatedHandler(typeof(StringArrayFilterHandler))]
public class StringArrayFilter : ArrayFilter<string>
{
}
