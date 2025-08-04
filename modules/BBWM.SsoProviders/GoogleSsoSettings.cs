using BBWM.SystemSettings;

namespace BBWM.SsoProviders;

public class GoogleSsoSettings : IMutableSystemConfigurationSettings
{
    /// <summary>
    /// Google ClientId
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Google ClientSecret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Google SSO Enabled
    /// </summary>
    public bool? Enabled { get; set; } = false;
}
