using BBWM.Core.Filters.TypedFilters;

namespace BBWM.Core.Filters.Handlers;

public class NumberFilterHandler : CountableFilterHandler<double>
{
    public NumberFilterHandler(NumberFilter filter) : base(filter)
    {
    }
}
