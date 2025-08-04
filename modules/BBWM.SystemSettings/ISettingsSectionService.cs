using System.Text.Json;

namespace BBWM.SystemSettings;

public interface ISettingsSectionService
{
    JsonSerializerOptions JsonSerializerOptions { get; }

    void SetJsonSerializerSettings(JsonSerializerOptions options);

    void RegisterSection<T>(string sectionName);

    bool HasSection(string sectionName);

    bool HasSection(Type type);

    Type GetSectionType(string sectionName);

    string GetSectionName(Type type);
}