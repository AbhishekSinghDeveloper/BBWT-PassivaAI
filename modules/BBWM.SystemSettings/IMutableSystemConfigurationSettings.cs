namespace BBWM.SystemSettings;

/// <summary>
/// The interface serves decorative purposes only. It marks an instance of a system configuration settings as mutable through the front end.
/// IMPORTANT: These settings should not contain properties that do not allow the "null" value because of the saving specific.
/// Otherwise, it may lead to serious internal conflict that may crush the entire application working.
/// </summary>
public interface IMutableSystemConfigurationSettings
{
}
