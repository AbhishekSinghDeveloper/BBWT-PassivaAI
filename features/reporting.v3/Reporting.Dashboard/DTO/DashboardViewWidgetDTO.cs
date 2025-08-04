using BBWM.Core.DTO;

namespace BBF.Reporting.Dashboard.DTO;

public class DashboardViewWidgetDTO : IDTO
{
    public int Id { get; set; }
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }

    // Foreign keys and navigational properties.
    public Guid WidgetSourceId { get; set; }
    public string WidgetType { get; set; } = null!;
}