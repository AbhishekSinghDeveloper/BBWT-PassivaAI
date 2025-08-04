namespace BBWM.Core.Membership.Model;

public class UserOrganizationGroup
{
    public string UserId { get; set; }
    public int OrganizationId { get; set; }
    public UserOrganization UserOrganization { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; }
}
