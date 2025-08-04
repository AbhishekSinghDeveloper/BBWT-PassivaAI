using BBWM.Core.Membership.DTO;

namespace BBWM.Demo.ReportingV3;

public static class InitialUsers
{
    public static readonly UserDTO ReportAdmin = new()
    {
        Email = "report-admin@bbconsult.co.uk",
        Password = "keep842calmness",
        FirstName = "Report",
        LastName = "Admin",
    };
};
