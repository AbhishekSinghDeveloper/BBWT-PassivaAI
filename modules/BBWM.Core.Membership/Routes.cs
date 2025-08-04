using BBWM.Core.Web;

namespace BBWM.Core.Membership;

public static class Routes
{
    public static Route LoginAudit => new("/app/admin/login-audit", "Login Audit");
    public static Route Users => new("/app/users", "Users");
    public static Route UsersDetails => new("/app/users/edit/:id", "Edit User");
    public static Route Roles => new("/app/roles", "Roles");
    public static Route Organizations => new("/app/organizations", "Organizations");
    public static Route OrganizationDetails => new("/app/organizations/edit/:id", "Organization Details");
    public static Route AllowedIpDetails => new("/app/allowed-ip/edit/:id", "Allowed Ip");
    public static Route RoutesAccess => new("/app/routes", "Routes Access");
}
