namespace BBWM.AggregatedLogs
{
    public interface IFileProvider
    {
        IEnumerable<string> ReadLines(string path);

        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption, DateTimeOffset lastTimestamp);
    }
}
