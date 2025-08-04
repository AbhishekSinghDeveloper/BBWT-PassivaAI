using BBWM.Core.DTO;

namespace BBWM.Core.Membership.DTO;

public class OrganizationDTO : IDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    public string PostCode => Address?.PostCode;

    public int? AddressId { get; set; }
    public AddressDTO Address { get; set; }
    public int? BrandingId { get; set; }
    public BrandingDTO Branding { get; set; }
}
