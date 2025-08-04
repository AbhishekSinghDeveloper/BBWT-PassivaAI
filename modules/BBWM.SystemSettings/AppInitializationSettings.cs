namespace BBWM.SystemSettings;

public class AppInitializationSettings : IMutableSystemConfigurationSettings
{
    /// <summary>
    /// Indicates whether once seeded data has been ever initialized. Initialization happens on app start up,
    /// then the flag is set to True and thereafter the initialization should never happen again unless the flag
    /// is dropped manually. This flag is needed to, for example, seed initial users DB records.
    /// </summary>
    public bool? OnceSeededDataInitialized { get; set; } = false;
}
