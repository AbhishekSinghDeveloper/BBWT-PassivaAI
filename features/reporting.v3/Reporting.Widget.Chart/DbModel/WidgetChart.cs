using BBF.Reporting.Core.DbModel;
using BBWM.Core.Data;

namespace BBF.Reporting.Widget.Chart.DbModel;

public class WidgetChart : IAuditableEntity, IWidgetEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Configuration of the chart, in Json format.
    /// </summary>
    public string? ChartSettingsJson { get; set; }

    // Foreign key and navigational properties.
    public Guid? QuerySourceId { get; set; }
    public Guid WidgetSourceId { get; set; }

    public QuerySource? QuerySource { get; set; }
    public WidgetSource WidgetSource { get; set; } = null!;

    public IList<WidgetChartColumn> Columns { get; set; } = new List<WidgetChartColumn>();
}