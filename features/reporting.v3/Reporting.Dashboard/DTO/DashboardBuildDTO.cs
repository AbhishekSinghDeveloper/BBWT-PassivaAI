using BBF.Reporting.Dashboard.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Dashboard.DTO;

public class DashboardBuildDTO : IDTO<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool DisplayName { get; set; }
    public string? Description { get; set; }
    public string? UrlSlug { get; set; }
    public LayoutType Layout { get; set; }
    public int WidgetsMargin { get; set; }
    public int WidgetsPadding { get; set; }

    // Foreign keys and navigational properties.
    public IList<DashboardBuildWidgetDTO> Widgets { get; set; } = new List<DashboardBuildWidgetDTO>();
}