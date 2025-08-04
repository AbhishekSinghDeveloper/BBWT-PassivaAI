namespace BBWM.Core.Filters;

public interface ISorter
{
    string SortingField { get; set; }
    OrderDirection? SortingDirection { get; set; }
}
