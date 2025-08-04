using BBWM.Core.Data;
using BBWM.Core.Membership.Enums;
using BBWM.FileStorage;

using Microsoft.AspNetCore.Identity;

namespace BBWM.Core.Membership.Model;


/// <summary>
/// User class definition
/// </summary>
public class User : IdentityUser, IAuditableEntity<string>
{
    public override string PhoneNumber { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public AccountStatus AccountStatus { get; set; }

    public AccountStatus? PreviousAccountStatus { get; set; }

    public int? SsoProvider { get; set; }

    public DateTimeOffset? FirstPasswordFailureDate { get; set; }

    public string GravatarImage { get; set; }

    public string GravatarEmail { get; set; }

    public PictureMode PictureMode { get; set; }

    public bool U2fEnabled { get; set; }

    public string RecoveryCode { get; set; }

    /// <summary>
    /// Supposed to be a default user organization. Whereas UserOrganizations table keeps a list of all organizations
    /// the user belongs to.
    /// </summary>
    public int? OrganizationId { get; set; }

    public Organization Organization { get; set; }

    public int? AvatarImageId { get; set; }

    public FileDetails AvatarImage { get; set; }

    public int? InvitationTokenId { get; set; }

    public ActivationToken InvitationToken { get; set; }

    public int? PasswordResetTokenId { get; set; }

    public ActivationToken PasswordResetToken { get; set; }

    public int? EmailConfirmationTokenId { get; set; }

    public ActivationToken EmailConfirmationToken { get; set; }


    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    /// <summary>
    /// A list of all organizations the user belongs to.
    /// </summary>
    public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public ICollection<AllowedIpUser> AllowedIpUser { get; set; } = new List<AllowedIpUser>();

    public ICollection<Device> DeviceRegistrations { get; set; } = new List<Device>();

    public ICollection<AuthenticationRequest> AuthenticationRequests { get; set; } = new List<AuthenticationRequest>();

    public string LastLoginBrowserFingerprint { get; set; }

    public string AuthSecurityStamp { get; set; }

    public string? UserSignatureJson { get; set; }
}
