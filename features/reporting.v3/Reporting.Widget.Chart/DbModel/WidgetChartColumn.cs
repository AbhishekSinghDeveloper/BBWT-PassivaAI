using BBWM.Core.Data;
using BBF.Reporting.Widget.Chart.Enums;

namespace BBF.Reporting.Widget.Chart.DbModel;

public class WidgetChartColumn : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Alias of a query column which identifies this column.
    /// </summary>
    public string QueryAlias { get; set; } = null!;

    /// <summary>
    /// Alias of a chart column which identifies this column.
    /// </summary>
    public string? ChartAlias { get; set; }

    /// <summary>
    /// Purpose of this column (what it will be used for).
    /// </summary>
    public ColumnPurpose ColumnPurpose { get; set; }

    // Foreign key and navigational properties.
    public int ChartId { get; set; }
    public WidgetChart Chart { get; set; } = null!;
}