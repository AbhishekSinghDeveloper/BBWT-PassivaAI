namespace BBWM.Core.Membership.Model;

public class UserOrganization
{
    public string UserId { get; set; }
    public User User { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public ICollection<UserOrganizationGroup> Groups { get; set; } = new HashSet<UserOrganizationGroup>();
}
