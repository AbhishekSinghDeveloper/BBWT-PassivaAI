using BBWM.SystemSettings;

namespace BBWM.Maintenance;


public enum MaintenanceOptions
{
    Basic = 0,
    External = 1
}

/// <summary>
/// Represents maintenance mode settings.
/// </summary>
public class MaintenanceSettings : IMutableSystemConfigurationSettings
{
    public MaintenanceOptions? Option { get; set; } = MaintenanceOptions.Basic;

    public DateTime? Start { get; set; } = DateTime.Today.AddDays(-1);

    public DateTime? End { get; set; } = DateTime.Today.AddMinutes(-1);

    public string Message { get; set; } = "The site is currently under maintenance. Thank you for your patience.";

    public bool? IsActive { get; set; }

    public string ExternalApiUrl { get; set; }
}
