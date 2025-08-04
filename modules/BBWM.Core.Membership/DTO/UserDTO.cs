using BBWM.Core.DTO;
using BBWM.Core.Membership.Enums;
using BBWM.FileStorage;

using Destructurama.Attributed;

namespace BBWM.Core.Membership.DTO;

public class UserDTO : IDTO<string>
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public int? SsoProvider { get; set; }
    public string PhoneNumber { get; set; }
    [NotLogged] public string Password { get; set; }
    [NotLogged] public string ConfirmPassword { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool IsUserRequiredSetupTwoFactor { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool U2fEnabled { get; set; }

    //public string RecoveryCode { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsSystemAdmin { get; set; }
    public bool IsSystemTester { get; set; }
    public PictureMode PictureMode { get; set; }
    public string GravatarImage { get; set; }
    public string GravatarEmail { get; set; }

    /// <summary>
    /// Supposed to be a default user organization. Whereas UserOrganizations table keeps a list of all organizations
    /// the user belongs to.
    /// </summary>
    public int? OrganizationId { get; set; }
    public OrganizationDTO Organization { get; set; }
    public int? AvatarImageId { get; set; }
    public FileDetailsDTO AvatarImage { get; set; }

    public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    public ICollection<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
    public ICollection<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
    public ICollection<GroupDTO> Groups { get; set; } = new List<GroupDTO>();
    public ICollection<OrganizationDTO> Organizations { get; set; } = new List<OrganizationDTO>();
}
