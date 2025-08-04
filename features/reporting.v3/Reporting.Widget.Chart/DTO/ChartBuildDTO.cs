using BBF.Reporting.Core.DTO;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.Chart.DTO;

public class ChartBuildDTO : IDTO
{
    public int Id { get; set; }
    public string? ChartSettingsJson { get; set; }

    // Foreign keys and navigational properties.
    public Guid? QuerySourceId { get; set; }
    public Guid WidgetSourceId { get; set; }

    public WidgetSourceDTO WidgetSource { get; set; } = null!;

    public IList<ChartBuildColumnDTO> Columns { get; set; } = new List<ChartBuildColumnDTO>();
}