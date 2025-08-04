using BBF.Reporting.Core.DTO;
using BBWM.Core.DTO;

namespace BBF.Reporting.Dashboard.DTO;

public class DashboardBuildWidgetDTO : IDTO
{
    public int Id { get; set; }
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }

    // Foreign keys and navigational properties.
    public Guid WidgetSourceId { get; set; }
    public WidgetSourceDTO WidgetSource { get; set; } = null!;
}