namespace BBWM.Core.Membership.Interfaces;

public interface IAllowedIpService
{
    /// <summary>
    /// Checks whether a feature that checks alloded IP addresses is active.
    /// </summary>
    Task<bool> IsServiceActive(CancellationToken ct = default);

    /// <summary>
    /// Checks whether IP address allowed for specified allowed IP settings of the user or allowed IP settings of user's roles.
    /// </summary>
    /// <param name="ip">The IP address that should be checked.</param>
    /// <param name="userId">The identifier of user who settings should be used.</param>
    Task<bool> IsIpAllowedForUser(string ip, string userId, CancellationToken cancellationToken = default);
}