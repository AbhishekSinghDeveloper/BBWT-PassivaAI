namespace BBWM.AggregatedLogs;

public interface ILogLineParser
{
    Log Parse(string line, string appName, string serverName, string sourceName);
}
