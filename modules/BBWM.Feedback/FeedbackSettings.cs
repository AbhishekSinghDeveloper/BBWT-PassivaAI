using BBWM.SystemSettings;

namespace BBWM.Feedback;

/// <summary>
/// Represents the feedback settings.
/// </summary>
public class FeedbackSettings : IMutableSystemConfigurationSettings
{
    public bool? Enabled { get; set; }
}
