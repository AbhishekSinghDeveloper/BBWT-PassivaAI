namespace BBWM.Core.Loggers;

public class GraylogSettings
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

    public static Serilog.Events.LogEventLevel ParseLogEventLevel(string s, Serilog.Events.LogEventLevel defLevel)
        => (s?.ToLower()) switch
        {
            "Error" => Serilog.Events.LogEventLevel.Error,
            "Warning" => Serilog.Events.LogEventLevel.Warning,
            "Debug" => Serilog.Events.LogEventLevel.Debug,
            "Verbose" => Serilog.Events.LogEventLevel.Verbose,
            "Fatal" => Serilog.Events.LogEventLevel.Fatal,
            "Information" => Serilog.Events.LogEventLevel.Information,
            _ => defLevel,
        };
}
