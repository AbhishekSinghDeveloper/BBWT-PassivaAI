using BBWM.Core.Filters.TypedFilters;

namespace BBWM.Core.Filters.Handlers
{
    public class StringArrayFilterHandler : ArrayFilterHandler<string>
    {
        public StringArrayFilterHandler(ArrayFilter<string> filter) : base(filter)
        {
        }
    }
}
