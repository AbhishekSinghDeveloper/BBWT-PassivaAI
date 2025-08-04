using BBWM.Core.Filters.Handlers;

namespace BBWM.Core.Filters.TypedFilters;

public enum StringFilterMatchMode
{
    Contains = 0,
    NotContains = 1,
    StartsWith = 2,
    EndsWith = 3,
    Equals = 4,
    NotEquals = 5,
    In = 6
}

[RelatedHandler(typeof(StringFilterHandler))]
public class StringFilter : FilterInfoBase<string>
{
    public StringFilterMatchMode MatchMode { get; set; }
}
