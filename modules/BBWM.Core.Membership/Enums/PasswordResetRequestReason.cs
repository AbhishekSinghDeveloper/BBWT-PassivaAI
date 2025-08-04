namespace BBWM.Core.Membership.Enums;

/// <summary>
/// A list of reasons why user is required to reset password on login.
/// </summary>
public enum PasswordResetRequestReason
{
    /// <summary>
    /// Reason not specified
    /// </summary>
    None,
    /// <summary>
    /// Reason is to reset initially seeded account's password which is hardcoded and shouldn't persist
    /// in a live system.
    /// </summary>
    InitialAccountReset
};