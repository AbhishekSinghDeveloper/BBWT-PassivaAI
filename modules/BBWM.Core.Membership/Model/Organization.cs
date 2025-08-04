using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class Organization : IAuditableEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Level { get; set; }


    public Address Address { get; set; }

    public int? AddressId { get; set; }

    public Branding Branding { get; set; }

    public int? BrandingId { get; set; }

    public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();

}
