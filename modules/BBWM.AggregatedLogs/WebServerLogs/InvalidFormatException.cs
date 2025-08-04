namespace BBWM.AggregatedLogs;

public class InvalidFormatException : Exception
{
    public InvalidFormatException(string line) : base($"Invalid web server log line format: {line}")
    { }
}
