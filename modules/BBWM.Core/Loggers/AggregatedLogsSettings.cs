using BBWM.Core.Data;

namespace BBWM.Core.Loggers;

public class AggregatedLogsSettings : IDatabaseConnectionSettings
{
    public const string AggregatedLogsSettingsDefaultSectionName = "AggregatedLogsSettings";

    public const string TableName = "Logs";

    private DatabaseType? _databaseType;
    private int? _maxRetryCount;
    private int? _maxRetryDelay;
    private int? _period;
    private int? _batchLimit;

    /// <summary>
    /// Enables/Disables aggregated logging
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The type of the database to use
    /// </summary>
    public DatabaseType DatabaseType
    {
        get => _databaseType ?? DatabaseType.MySql;
        set => _databaseType = value;
    }

    /// <summary>
    /// The maximum number of connection retry attempts
    /// </summary>
    public int MaxRetryCount
    {
        get => _maxRetryCount ?? 10;
        set => _maxRetryCount = value;
    }

    /// <summary>
    /// The maximum delay between connection retries
    /// </summary>
    public int MaxRetryDelay
    {
        get => _maxRetryDelay ?? 30;
        set => _maxRetryDelay = value;
    }

    /// <summary>
    /// Additional SQL error numbers that should be considered transient
    /// </summary>
    public int[] ErrorNumbersToAdd { get; set; }

    /// <summary>
    /// Level of logs detailing (Verbose/Debug/Information/Warning/Error/Fatal)
    /// </summary>
    public string LogEventLevel { get; set; }

    /// <summary>
    /// Database connection string
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Period to wait before checking for next batch (in seconds)
    /// </summary>
    public int Period
    {
        get => _period ?? 30;
        set => _period = value;
    }

    /// <summary>
    /// Maximum number of events written in batch
    /// </summary>
    public int BatchLimit
    {
        get => _batchLimit ?? 50;
        set => _batchLimit = value;
    }
}
