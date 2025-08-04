using BBWM.Core.Membership.Model;

namespace BBWM.Reporting.Model;

public class ReportPermission
{
    public Guid ReportId { get; set; }
    public Report Report { get; set; }

    public int PermissionId { get; set; }
    public Permission Permission { get; set; }
}
