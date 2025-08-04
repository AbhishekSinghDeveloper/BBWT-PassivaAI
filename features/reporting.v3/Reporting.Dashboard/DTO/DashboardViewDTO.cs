using BBF.Reporting.Dashboard.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Dashboard.DTO;

public class DashboardViewDTO : IDTO<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool DisplayName { get; set; }
    public LayoutType Layout { get; set; }
    public int WidgetsMargin { get; set; }
    public int WidgetsPadding { get; set; }

    // Foreign keys and navigational properties.
    public IList<DashboardViewWidgetDTO> Widgets { get; set; } = new List<DashboardViewWidgetDTO>();
}