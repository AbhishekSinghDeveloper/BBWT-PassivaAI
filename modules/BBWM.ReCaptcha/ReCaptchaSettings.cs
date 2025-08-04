using BBWM.SystemSettings;

namespace BBWM.ReCaptcha;

/// <summary>
/// Represents the settings of Google ReCaptcha.
/// </summary>
public class ReCaptchaSettings : IMutableSystemConfigurationSettings
{
    public bool? ValidateOnLoginEnabled { get; set; } = false;

    public int? LockIntervalInSeconds { get; set; } = 5;
}
