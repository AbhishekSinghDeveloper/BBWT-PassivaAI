using BBF.Reporting.Dashboard.Enums;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;

namespace BBF.Reporting.Dashboard.DbModel;

public class Dashboard : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool DisplayName { get; set; }
    public string? Description { get; set; }
    public string? UrlSlug { get; set; }
    public DateTime CreatedOn { get; set; }
    public LayoutType Layout { get; set; }
    public int WidgetsMargin { get; set; }
    public int WidgetsPadding { get; set; }

    // Foreign keys and navigational properties.
    public string? OwnerId { get; set; } = null!;
    public User? Owner { get; set; } = null!;

    public ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}