using BBWM.SystemSettings;

namespace BBWM.Core.Membership.SystemSettings;

public enum TwoFactorMandatoryMode
{
    /// <summary>
    /// User can login without 2FA if it's disabled for his account. User can enable 2FA manually anyway if he wants.
    /// </summary>
    Optional,

    /// <summary>
    /// User must set up 2FA for his account if he wants to use the site
    /// </summary>
    Mandatory,

    /// <summary>
    /// Each role have it's own 2FA setting (two variants below for every role, by default each role have disabled 2FA i.e. TwoFactorOptional)
    /// </summary>
    MandatoryForSpecificRoles
}

/// <summary>
/// Represents settings for the 2FA.
/// </summary>
public class TwoFactorSettings : IMutableSystemConfigurationSettings
{
    public TwoFactorMandatoryMode? MandatoryMode { get; set; } = TwoFactorMandatoryMode.Optional;

    /// <summary>
    /// Specifies (in minutes) how long after successful 2FA login user can login without 2FA
    /// </summary>
    public int? AuthDurationMinutesOnLogin { get; set; } = 240;

    public int? AuthDurationMinutesForSettings { get; set; } = 5;
}
