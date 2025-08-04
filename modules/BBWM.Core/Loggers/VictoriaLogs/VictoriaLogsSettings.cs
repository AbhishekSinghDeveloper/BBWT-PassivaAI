namespace BBWM.Core.Loggers.VictoriaLogs;

public class VictoriaLogsSettings
{
    /// <summary>
    /// Enables/Disables Greylog logging.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Project name, added to Greylog records as property.
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// Log event level (Verbose/Debug/Information/...)
    /// </summary>
    public string LogEventLevel { get; set; }
    
}
