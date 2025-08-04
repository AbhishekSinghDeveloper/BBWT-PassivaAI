using BBF.Reporting.Widget.Chart.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.Chart.DTO;

public class ChartViewColumnDTO : IDTO
{
    public int Id { get; set; }

    public string QueryAlias { get; set; } = null!;
    public string? ChartAlias { get; set; }
    public ColumnPurpose ColumnPurpose { get; set; }

    // Foreign key and navigational properties.
    public int ChartId { get; set; }
}