using BBWM.Core.DTO;
using BBWM.Core.Membership.DTO;

namespace BBWM.Reporting.DTO;

public class ReportDTO : IDTO<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// Report name.
    /// </summary>
    public string Name { get; set; }

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

    public Guid? PublishedReportId { get; set; }

    public ReportDTO PublishedReport { get; set; }


    /// <summary>
    /// Roles for a report.
    /// </summary>
    public IList<RoleDTO> Roles { get; set; } = new List<RoleDTO>();

    /// <summary>
    /// Permissions for a report.
    /// </summary>
    public IList<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();

    public IList<SectionDTO> Sections { get; set; } = new List<SectionDTO>();
}
