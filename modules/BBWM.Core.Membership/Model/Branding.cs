using BBWM.Core.Data;
using BBWM.FileStorage;

namespace BBWM.Core.Membership.Model;

public class Branding : IAuditableEntity
{
    public int Id { get; set; }

    public string Theme { get; set; }

    public string EmailBody { get; set; }

    public bool Disabled { get; set; }


    public int? LogoImageId { get; set; }

    public FileDetails LogoImage { get; set; }

    public int? LogoIconId { get; set; }

    public FileDetails LogoIcon { get; set; }

    [DoNotRequireChildId]
    public Organization Organization { get; set; }
}
