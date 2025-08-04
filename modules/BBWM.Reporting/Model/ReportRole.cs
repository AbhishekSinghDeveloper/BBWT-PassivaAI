using BBWM.Core.Membership.Model;

namespace BBWM.Reporting.Model;

public class ReportRole
{
    public Guid ReportId { get; set; }
    public Report Report { get; set; }

    public string RoleId { get; set; }
    public Role Role { get; set; }
}
