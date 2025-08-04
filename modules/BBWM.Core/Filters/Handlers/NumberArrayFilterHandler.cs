using BBWM.Core.Filters.TypedFilters;

namespace BBWM.Core.Filters.Handlers
{
    public class NumberArrayFilterHandler : ArrayFilterHandler<int>
    {
        public NumberArrayFilterHandler(ArrayFilter<int> filter) : base(filter)
        {
        }
    }
}
