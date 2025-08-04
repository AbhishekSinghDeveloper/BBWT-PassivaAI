namespace BBWM.AggregatedLogs;

public class WebServerLogsSettings
{
    public const string WebServerSettingsDefaultSectionName = "WebServerLogsSettings";

    /// <summary>
    /// Path to folder with NCSA logs to parse
    /// </summary>
    public string FolderPath { get; set; }

    /// <summary>
    /// Source to specify for web server logs (IIS, nginx etc)
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// App name to specify for web server logs
    /// </summary>
    public string AppName { get; set; }
}
