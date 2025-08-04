using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class Group : IAuditableEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    public ICollection<UserOrganizationGroup> UserOrganizationGroups { get; set; } = new List<UserOrganizationGroup>();
}
