namespace BBWM.SystemSettings;

public interface ISettingsService
{
    Task<SettingsDTO[]> Load(SettingsName[] settingsNames = null);
    Task<SettingsDTO[]> Save(SettingsDTO[] config);
    void SaveSettingsSection<T>(T config) where T : class;
    T GetSettingsSection<T>() where T : class;
}
