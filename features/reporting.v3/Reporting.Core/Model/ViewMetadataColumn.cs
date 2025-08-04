namespace BBF.Reporting.Core.Model;

public class ViewMetadataColumn
{
    /// <summary>
    /// E.g. for grid widget column's cell format or for chart widget dataset's point info
    /// </summary>
    public string Mask { get; set; } = null!;

    /// <summary>
    /// E.g. for column width in table view of widget
    /// </summary>
    public float? Width { get; set; }

    /// <summary>
    /// E.g. for column width in table view of widget
    /// </summary>
    public float? MinWidth { get; set; }

    /// <summary>
    /// E.g. for column width in table view of widget
    /// </summary>
    public float? MaxWidth { get; set; }

    /// <summary>
    /// Identifier that represents a column
    /// </summary>
    public string QueryAlias { get; set; } = null!;

    /// <summary>
    /// E.g. for column headers in widgets
    /// </summary>
    public string Title { get; set; } = null!;
}
