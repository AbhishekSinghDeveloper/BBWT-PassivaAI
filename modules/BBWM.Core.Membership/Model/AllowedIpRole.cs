using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class AllowedIpRole : IAuditableEntity
{
    public int Id { get; set; }

    public virtual AllowedIp AllowedIp { get; set; }

    public int AllowedIpId { get; set; }

    public virtual Role Role { get; set; }

    public string RoleId { get; set; }
}
