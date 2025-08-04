using BBWM.Core.Data;
using BBWM.Core.Extensions;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using System.Text.Json;

namespace BBWM.SystemSettings;

public class SettingsService : ISettingsService
{
    private readonly IDbContext _dataContext;
    private readonly ISettingsSectionService _sectionsService;
    private readonly ILogger<SettingsService> _logger;

    //TODO: to review where we should store this key?!
    private readonly string key = "E546C8DF278CD5931069B522E695D4F2";

    public SettingsService(
        IDbContext dataContext,
        ISettingsSectionService sectionService,
        ILogger<SettingsService> logger)
    {
        _dataContext = dataContext;
        _sectionsService = sectionService;
        _logger = logger;
    }

    public async Task<SettingsDTO[]> Load(SettingsName[] settingsNames = null)
    {
        var settings = GetCurrentConfig(settingsNames).ToArray();
        foreach (var settingsItem in settings)
        {
            try
            {
                DecryptSettingsSection(settingsItem);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Cannot read a system settings section '{settingsItem?.SectionName}'");
            }
        }

        return settings;
    }

    public async Task<SettingsDTO[]> Save(SettingsDTO[] config)
    {
        var dbSet = _dataContext.Set<AppSettings>();
        var sectionNames = new List<AppSettings>();

        foreach (var configItem in config)
        {
            var sectionName = string.IsNullOrEmpty(configItem.SectionName) ? GetSectionName(configItem.Value.GetType()) : configItem.SectionName;

            var section = string.IsNullOrEmpty(sectionName) ?
                null : dbSet.FirstOrDefault(s => s.Section.Equals(sectionName));
            if (section is null)
            {
                section = new AppSettings
                {
                    Section = sectionName,
                    Value = JsonSerializer.Serialize(configItem.Value, _sectionsService.JsonSerializerOptions)
                };
                dbSet.Add(section);
            }
            else
            {
                EncryptSettingsValue(configItem.Value, section.EncryptedFields);
                section.Value = JsonSerializer.Serialize(configItem.Value, _sectionsService.JsonSerializerOptions);
            }

            var settingsType = GetSectionType(sectionName);
            if (settingsType is not null)
            {
                try
                {
                    JsonSerializer.Deserialize(section.Value, settingsType, _sectionsService.JsonSerializerOptions);
                }
                catch (Exception ex)
                {
                    throw new ValidationException($"The section '{sectionName}' passed for saving has an invalid data format: {ex.Message}");
                }
            }

            sectionNames.Add(section);
        }

        await _dataContext.SaveChangesAsync();

        return ConvertAppSettingsToSettingDTOs(sectionNames.ToArray()).ToArray();
    }

    public T GetSettingsSection<T>() where T : class
    {
        var sectionName = GetSectionName<T>();

        try
        {
            var data = string.IsNullOrEmpty(sectionName)
                    ? null
                    : _dataContext.Set<AppSettings>().FirstOrDefault(s =>
                        string.Equals(s.Section, sectionName));
            if (data is not null)
            {
                var obj = JsonSerializer.Deserialize<T>(data.Value, _sectionsService.JsonSerializerOptions);
                DecryptSettingsValue(obj, data.EncryptedFields);
                return obj;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Cannot read a system settings section '{sectionName}'");
        }

        return Activator.CreateInstance(typeof(T)) as T;
    }

    public void SaveSettingsSection<T>(T config) where T : class
    {
        var sectionName = GetSectionName<T>();
        if (!string.IsNullOrEmpty(sectionName))
        {
            var dbSetting = _dataContext.Set<AppSettings>().FirstOrDefault(s => s.Section == sectionName) ?? new AppSettings { Section = sectionName };
            EncryptSettingsValue(config, dbSetting.EncryptedFields);
            dbSetting.Value = JsonSerializer.Serialize(config, _sectionsService.JsonSerializerOptions);
            if (dbSetting.Id == 0)
            {
                _dataContext.Set<AppSettings>().Add(dbSetting);
            }

            _dataContext.SaveChanges();
        }
    }

    private Type GetSectionType(string sectionName) =>
        _sectionsService.HasSection(sectionName)
            ? _sectionsService.GetSectionType(sectionName)
            : null;

    private string GetSectionName<T>() => GetSectionName(typeof(T));

    private string GetSectionName(Type type) =>
        _sectionsService.HasSection(type)
            ? _sectionsService.GetSectionName(type)
            : null;

    private IEnumerable<SettingsDTO> GetCurrentConfig(SettingsName[] settingsNames = null)
    {
        var settings = _dataContext.Set<AppSettings>().AsQueryable();
        if (settingsNames is not null)
            settings = settings.Where(settingsItem => settingsNames.Select(settingsNameItem => settingsNameItem.ToString()).Contains(settingsItem.Section));

        return ConvertAppSettingsToSettingDTOs(settings.ToArray());
    }

    private IEnumerable<SettingsDTO> ConvertAppSettingsToSettingDTOs(AppSettings[] settings)
    {
        var result = new List<SettingsDTO>();

        foreach (var settingsItems in settings)
        {
            var sectionType = GetSectionType(settingsItems.Section);

            if (sectionType is null) continue;

            object? value = null;
            try
            {
                value = JsonSerializer.Deserialize(settingsItems.Value, sectionType, _sectionsService.JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deserializing a string value of '{settingsItems.Section}'");
            }

            value ??= Activator.CreateInstance(sectionType);

            result.Add(new SettingsDTO
            {
                SectionName = settingsItems.Section,
                Value = value
            });
        }

        return result;
    }

    private void DecryptSettingsSection(SettingsDTO settingsSection)
    {
        var dbSet = _dataContext.Set<AppSettings>();
        var sectionName = settingsSection.SectionName;
        var section = string.IsNullOrEmpty(sectionName) ?
            null : dbSet.FirstOrDefault(s => s.Section.Equals(sectionName));
        if (section is not null)
        {
            DecryptSettingsValue(settingsSection.Value, section.EncryptedFields);
        }
    }

    private void DecryptSettingsValue(object value, string fields)
    {
        if (string.IsNullOrEmpty(fields) || value is null) return;

        var fieldsList = fields.Split(',').ToList();
        foreach (var field in fieldsList)
        {
            var changingField = value.GetType().GetRuntimeProperty(field);
            if (changingField is not null)
            {
                var newValue = (string)changingField.GetValue(value);

                if (!string.IsNullOrEmpty(newValue))
                {
                    newValue = newValue.AesDecryptBase64(key);
                    changingField.SetValue(value, newValue);
                }
            }
        }
    }

    private void EncryptSettingsValue(object value, string fields)
    {
        if (string.IsNullOrEmpty(fields) || value is null) return;

        var fieldsList = fields.Split(',').ToList();
        foreach (var field in fieldsList)
        {
            var changingField = value.GetType().GetRuntimeProperty(field);
            if (changingField is not null)
            {
                var newValue = (string)changingField.GetValue(value);

                if (!string.IsNullOrEmpty(newValue))
                {
                    newValue = newValue.AesEncryptBase64(key);
                    changingField.SetValue(value, newValue);
                }
            }
        }
    }
}