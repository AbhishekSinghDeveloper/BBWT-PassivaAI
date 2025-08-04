using BBWM.Core.Data;
using BBWM.Core.Membership.Model;

namespace BBWM.Reporting.Model;

/// <summary>
/// Reports table
/// </summary>
public class Report : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// Report name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Refers to the Name field which is actually plays a role of title in the report view.
    /// </summary>
    public bool ShowTitle { get; set; }

    /// <summary>
    /// Report name for display on a menu.
    /// </summary>
    public string UrlSlug { get; set; }

    /// <summary>
    /// Report access type
    /// "Authenticated" (AggregatedRole.Authenticated) - for authenticated users
    /// null/empty - for specific roles/permissions
    /// </summary>
    public string Access { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }

    public bool IsDraft { get; set; }


    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public User UpdatedByUser { get; set; }

    public Guid? PublishedReportId { get; set; }

    public Report PublishedReport { get; set; }


    /// <summary>
    /// Roles for a report.
    /// </summary>
    public IList<ReportRole> ReportRoles { get; set; } = new List<ReportRole>();

    /// <summary>
    /// Permissions for a report.
    /// </summary>
    public IList<ReportPermission> ReportPermissions { get; set; } = new List<ReportPermission>();

    public IList<Section> Sections { get; set; } = new List<Section>();
}
