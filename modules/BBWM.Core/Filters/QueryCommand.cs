namespace BBWM.Core.Filters;

public class QueryCommand : Filter, IPager, ISorter
{
    /// <summary>
    /// Name of the field to sort by.
    /// </summary>
    public string SortingField { get; set; }

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
