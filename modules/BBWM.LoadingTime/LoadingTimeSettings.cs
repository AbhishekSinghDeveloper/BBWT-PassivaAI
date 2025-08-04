using BBWM.SystemSettings;

namespace BBWM.LoadingTime;

/// <summary>
/// Represents the pages loading time recording settings.
/// </summary>
public class LoadingTimeSettings : IMutableSystemConfigurationSettings
{
    public bool? RecordLoadingTime { get; set; } = true;
}
