using BBF.Reporting.Core.ModelBinders;
using BBWM.Core.Filters;

namespace BBF.Reporting.Core.Model;

public class PagedGridSettings
{
    /// <summary>
    /// Name of the field to sort by.
    /// </summary>
    [UrlDecoded]
    public string? SortingField { get; set; }

    /// <summary>
    /// Sorting direction.
    /// </summary>
    public OrderDirection? SortingDirection { get; set; }

    /// <summary>
    /// Number of records to skip.
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Number of records to take.
    /// </summary>
    public int? Take { get; set; }
}