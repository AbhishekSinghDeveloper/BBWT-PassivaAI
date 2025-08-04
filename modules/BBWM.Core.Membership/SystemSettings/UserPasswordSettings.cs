using BBWM.SystemSettings;

namespace BBWM.Core.Membership.SystemSettings;


/// <summary>
/// Settings of User Password Reuse
/// </summary>
public enum PasswordReuseSettings
{
    /// <summary>
    /// Users may re-use passwords
    /// </summary>
    MayUse,

    /// <summary>
    /// Users may never use a password that they previously used
    /// </summary>
    NeverUse,

    /// <summary>
    /// Users may use any password that they haven’t used in the last N passwords
    /// </summary>
    MayReUse
}

/// <summary>
/// Represents restrictions of the password.
/// </summary>
public class UserPasswordSettings : IMutableSystemConfigurationSettings
{
    public const int DefaultPasswordResetExpireInDays = 1;


    /// <summary>
    /// Settings of User Password Reuse
    /// </summary>
    public PasswordReuseSettings? PasswordReuse { get; set; } = PasswordReuseSettings.MayReUse;

    /// <summary>
    /// Settings of Valid Characters
    /// </summary>
    public ValidCharactersSettings ValidCharacters { get; set; } = new ValidCharactersSettings();

    /// <summary>
    /// The number of last passwords that a user can reuse
    /// </summary>
    public int? LastPasswordsNumber { get; set; } = 5;

    /// <summary>
    /// Min Password Length
    /// </summary>
    public int? MinPasswordLength { get; set; } = 8;

    /// <summary>
    /// Password saving settings (on/off autocomplete for password/email inputs)
    /// </summary>
    public bool? Autocomplete { get; set; } = false;

    /// <summary>
    /// Common password minimal strength.
    /// </summary>
    public int? Strength { get; set; } = 2;

    /// <summary>
    /// Gets or sets how many days for a Password Reset Token to stay valid
    /// </summary>
    public int? PasswordResetTokenExpireInDays { get; set; } = DefaultPasswordResetExpireInDays;
}

/// <summary>
/// Represents the valid characters settings.
/// </summary>
/// <remarks>
/// Mutable through the front end. Do not use types that do not allow null value for properties of mutable settings class!
/// </remarks>
public class ValidCharactersSettings : IMutableSystemConfigurationSettings
{
    /// <summary>
    /// Lowercase
    /// </summary>
    public bool? Lowercase { get; set; } = true;

    /// <summary>
    /// Uppercase
    /// </summary>
    public bool? Uppercase { get; set; } = true;

    /// <summary>
    /// Digits
    /// </summary>
    public bool? Numbers { get; set; } = true;

    /// <summary>
    /// Punctuation
    /// </summary>
    public bool? Special { get; set; } = false;
}
