namespace BBWM.SystemSettings;

public class PwaSettings : IMutableSystemConfigurationSettings
{
    public bool? DesktopInstallationEnabled { get; set; } = true;

    public bool? MobileInstallationEnabled { get; set; } = true;

    public bool? DesktopShowIndicator { get; set; } = true;

    public bool? MobileShowIndicator { get; set; } = true;
}
