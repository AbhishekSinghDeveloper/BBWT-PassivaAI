using BBWM.Core.Membership.Model;

namespace BBWM.Core.Membership.Interfaces;

/// <summary>
/// Security service interface
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Returns the last locking record which have a most far unlocking date greater than now.
    /// </summary>
    /// <param name="ip">The IP address that checking performing for.</param>
    Task<LockedOutIp> GetLongestActiveLockingByIp(string ip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check IP address rely on audit history and lock it depends on settings and attempts count.
    /// </summary>
    /// <param name="ip">The IP that should be checked.</param>
    Task CheckIpLockOut(string ip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks the user if recaptcha failed and 2fa disabled/
    /// </summary>
    /// <param name="user">User to block</param>
    Task<bool> TryLockUserOnInvalidRecaptcha(User user, bool isReCaptchaValid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether IP address been locked.
    /// </summary>
    /// <param name="ip">The IP address that should be checked.</param>
    Task<bool> IsIpLocked(string ip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increases the failed attempts count for a specified user and locking him depends on settings and attempts count.
    /// </summary>
    /// <param name="user">The user who has input wrong credentials.</param>
    Task AddFailedAttemptForUser(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increases the failed attempts count for a specified user and locking him depends on settings and attempts count.
    /// </summary>
    /// <param name="userId">The identifier of user who has input wrong credentials.</param>
    Task AddFailedAttemptForUser(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs user unlocking.
    /// </summary>
    /// <param name="user">The user who should be unlocked.</param>
    Task UnlockUser(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks that the new password is valid for current system settings and existing user.
    /// </summary>
    /// <param name="user">The user a password should be checked for.</param>
    /// <param name="newPassword">New password.</param>
    /// <returns>A string that describes an error if any.</returns>
    string CheckUsersNewPassword(User user, string newPassword);

    /// <summary>
    /// Saves specified user's password to history.
    /// </summary>
    /// <param name="user">The user which password should be saved.</param>
    Task SavePasswordToHistory(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hashes specified string.
    /// </summary>
    /// <param name="value">The target string.</param>
    string GetHashedPassword(string value);
}
