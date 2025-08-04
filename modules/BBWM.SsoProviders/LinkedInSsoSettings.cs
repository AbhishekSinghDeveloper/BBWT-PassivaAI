using BBWM.SystemSettings;

namespace BBWM.SsoProviders;

public class LinkedInSsoSettings : IMutableSystemConfigurationSettings
{
    /// <summary>
    /// LinkedIn ClientId
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// LinkedIn ClientSecret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// LinkedIn SSO Enabled
    /// </summary>
    public bool? Enabled { get; set; } = false;
}
