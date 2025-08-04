using BBWM.SystemSettings;

namespace BBWM.Core.Membership.SystemSettings;


/// <summary>
/// Represents the behavior according to which the user will be locked out of the account.
/// </summary>
public enum LockType
{
    /// <summary>
    /// User are never locked out of the account due to failed attempts to enter his password.
    /// </summary>
    NeverLock = 0,

    /// <summary>
    /// User are locked out of the account after N consecutive failed attempts to enter their password within N minutes.
    /// </summary>
    AfterSeveralFailedAttempts
}

/// <summary>
/// Defines the behavior according to which the user will be unlocked.
/// </summary>
public enum UnlockType
{
    /// <summary>
    /// User will be locked out for N seconds.
    /// </summary>
    Temporary,

    /// <summary>
    /// User will be locked out until an administrator resets the password.
    /// </summary>
    ResetPassword
}

/// <summary>
/// Represents behavior settings related to failed login attempts.
/// </summary>
public class FailedAttemptsPasswordSettings : IMutableSystemConfigurationSettings
{
    /// <summary>
    /// Gets or sets the locks type.
    /// To pass the requirements of Cyber Essentials accreditation, users must not be permitted to make more than 10 login attempts within 5 minutes (300 seconds).
    /// </summary>
    public LockType? LockTypeAccount { get; set; } = LockType.AfterSeveralFailedAttempts;

    /// <summary>
    /// Gets or sets the unlocks type.
    /// </summary>
    public UnlockType? UnlockTypeAccount { get; set; } = UnlockType.Temporary;

    /// <summary>
    /// The number of invalid password attempts allowed before the user is locked out.
    /// </summary>
    public int? MaxInvalidPasswordAttempts { get; set; } = 5;

    /// <summary>
    /// The number of minutes in which a maximum number of invalid password attempts are allowed before the user is locked out.
    /// </summary>
    public int? PasswordAttemptWindow { get; set; } = 1;

    /// <summary>
    /// The number of seconds to lock a user account after the number of password attempts exceeds the value in the MaxInvalidPasswordAttempts parameter.
    /// </summary>
    public int? IntervalInSeconds { get; set; } = 150;
}
