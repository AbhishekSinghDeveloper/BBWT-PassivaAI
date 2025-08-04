using BBWM.Core.Data;

using Microsoft.AspNetCore.Identity;

namespace BBWM.Core.Membership.Model;

public class Role : IdentityRole, IAuditableEntity<string>
{
    public Role() { }

    public Role(string roleName) : base(roleName) { }


    public bool AuthenticatorRequired { get; set; }

    public bool CheckIp { get; set; }


    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();

    public ICollection<AllowedIpRole> AllowedIpRoles { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
