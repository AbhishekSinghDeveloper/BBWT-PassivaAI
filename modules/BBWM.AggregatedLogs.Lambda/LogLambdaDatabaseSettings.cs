using BBWM.Core.Data;

namespace BBWM.AggregatedLogs.Lambda
{
    public class LogLambdaDatabaseSettings : IDatabaseConnectionSettings
    {
        public const string SettingsSectionName = "DatabaseSettings";

        public const string TableName = "Logs";

        private DatabaseType? _databaseType;
        private int? _maxRetryCount;
        private int? _maxRetryDelay;

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
        /// Database connection string name
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
