namespace BBWM.DbDoc.DbGraph.Algorithms;

public class GraphPathSearchFilter
{
    /// <summary>
    /// Maximum path length to search in the graph. When 0 then no limit.
    /// </summary>
    public int MaxDepth { get; set; } = 0;

    /// <summary>
    /// Maximum total number of paths to search in the graph. When 0 then no limit.
    /// </summary>
    public int MaxTotal { get; set; } = 0;

    /// <summary>
    /// When True, self-join references are included in search, e.g. Reports.ParentReportId -> Reports.Id
    /// </summary>
    public bool IncludeSelfReferences { get; set; } = false;
}
