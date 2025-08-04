using BBWM.SystemSettings;

namespace BBWM.Core.Membership.SystemSettings;

/// <summary>
/// Represents the registration settings.
/// </summary>
public class RegistrationSettings : IMutableSystemConfigurationSettings
{
    public const int DefaultUserInvitationExpireInDays = 7;
    public const int DefaultEmailConfirmationExpireInDays = 7;


    /// <summary>
    /// Check on https://haveibeenpwned.com/
    /// </summary>
    public bool? CheckPwned { get; set; }

    public int? SelfRegisterUserOrganizationId { get; set; }

    /// <summary>
    /// Gets or sets how many days a User Invitation token is valid
    /// </summary>
    public int? UserInvitationExpireInDays { get; set; } = DefaultUserInvitationExpireInDays;

    public int? EmailConfirmationExpireInDays { get; set; } = DefaultEmailConfirmationExpireInDays;
}
