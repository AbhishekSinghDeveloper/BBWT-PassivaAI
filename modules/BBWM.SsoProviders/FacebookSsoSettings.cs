using BBWM.SystemSettings;

namespace BBWM.SsoProviders;

public class FacebookSsoSettings : IMutableSystemConfigurationSettings
{
    /// <summary>
    /// Facebook AppId 
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Facebook AppSecret
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;

    /// <summary>
    /// Facebook Enabled
    /// </summary>
    public bool? Enabled { get; set; } = false;
}
