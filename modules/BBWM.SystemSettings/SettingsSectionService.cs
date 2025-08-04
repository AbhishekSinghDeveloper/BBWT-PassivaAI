using System.Text.Json;

namespace BBWM.SystemSettings;

public class SettingsSectionService : ISettingsSectionService
{
    private readonly Dictionary<Type, string> _sections = new Dictionary<Type, string>();
    private JsonSerializerOptions _options;

    public void RegisterSection<T>(string sectionName) => _sections.Add(typeof(T), sectionName);

    public bool HasSection(string sectionName) => _sections.ContainsValue(sectionName);

    public bool HasSection(Type type) => _sections.ContainsKey(type);

    public Type GetSectionType(string sectionName) => _sections.FirstOrDefault(s => s.Value == sectionName).Key;

    public string GetSectionName(Type type) => _sections[type];

    public JsonSerializerOptions JsonSerializerOptions => _options;

    public void SetJsonSerializerSettings(JsonSerializerOptions options) => _options = options;
}