using BBWM.FileStorage;

namespace BBWM.SystemSettings;

/// <summary>
/// Represents site appearance settings.
/// </summary>
public class ProjectSettings : IMutableSystemConfigurationSettings
{
    public static readonly string DefaultName = "Blueberry Web Template v3";
    public static readonly string DefaultTheme = "ultima-indigo-compact";
    public static readonly string DefaultLogoIconUrl = "favicon.ico";
    public static readonly string DefaultLogoImageUrl = "/assets/images/logo.png";

    private string _theme;

    public string Name { get; set; } = DefaultName;

    public string ThemeCode
    {
        get => string.IsNullOrEmpty(_theme) ? DefaultTheme : _theme;
        set => _theme = value;
    }

    public int? LogoImageId { get; set; }

    public int? LogoIconId { get; set; }
}

public class ProjectSettingsImages
{
    public FileDetailsDTO LogoIcon { get; set; }
    public FileDetailsDTO LogoImage { get; set; }
}