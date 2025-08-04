using BBWM.SystemSettings;

namespace BBWM.Core.Membership.SystemSettings;

/// <summary>
/// Represents the login settings.
/// </summary>
public class UserLoginSettings : IMutableSystemConfigurationSettings
{
    public const string DefaultTwoFaAppName = "BBWT3";


    public string TwoFaAppName { get; set; } = DefaultTwoFaAppName;

    public bool? ShowNewBrowserLoginAlert { get; set; } = true;
}
