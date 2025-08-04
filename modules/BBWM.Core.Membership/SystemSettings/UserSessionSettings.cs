using BBWM.SystemSettings;

namespace BBWM.Core.Membership.SystemSettings;

/// <summary>
/// Represents the user's session timing settings.
/// </summary>
public class UserSessionSettings : IMutableSystemConfigurationSettings
{
    /// <summary>
    /// User's idle time (minutes). When user's inactivity period exceedes the parameter's value, the client-side application
    /// will trigger user logging out by sending request to the server.
    /// </summary>
    public int? IdleTime { get; set; } = 30;

    /// <summary>
    /// Determines if user's idle time option is enabled.
    /// </summary>
    public bool? IdleTimeEnabled { get; set; } = true;
}
