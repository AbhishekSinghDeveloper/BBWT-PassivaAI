using BBWM.Core.DTO;
using BBWM.FileStorage;

namespace BBWM.Core.Membership.DTO;

public class BrandingDTO : IDTO
{
    public int Id { get; set; }
    public string Theme { get; set; }
    public string EmailBody { get; set; }
    public bool Disabled { get; set; }

    public int? LogoImageId { get; set; }
    public FileDetailsDTO LogoImage { get; set; }
    public int? LogoIconId { get; set; }
    public FileDetailsDTO LogoIcon { get; set; }

    public OrganizationDTO Organization { get; set; }
}
