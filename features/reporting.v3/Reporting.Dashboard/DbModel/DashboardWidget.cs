using BBF.Reporting.Core.DbModel;
using BBWM.Core.Data;

namespace BBF.Reporting.Dashboard.DbModel;

public class DashboardWidget : IAuditableEntity
{
    public int Id { get; set; }
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }

    // Foreign keys and navigational properties.
    public Guid DashboardId { get; set; }
    public Guid WidgetSourceId { get; set; }

    public Dashboard Dashboard { get; set; } = null!;
    public WidgetSource WidgetSource { get; set; } = null!;
}