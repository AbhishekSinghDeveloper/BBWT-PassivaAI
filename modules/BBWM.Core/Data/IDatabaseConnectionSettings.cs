namespace BBWM.Core.Data
{
    public interface IDatabaseConnectionSettings
    {
        DatabaseType DatabaseType { get; }

        int MaxRetryCount { get; }

        int MaxRetryDelay { get; }

        int[] ErrorNumbersToAdd { get; set; }
    }
}
