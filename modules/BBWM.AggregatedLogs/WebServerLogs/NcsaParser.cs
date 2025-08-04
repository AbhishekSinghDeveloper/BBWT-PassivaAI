using BBWM.Core.Loggers;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BBWM.AggregatedLogs;

/// <summary>
/// Parser for Common Log Format lines (NCSA)
/// </summary>
public class NcsaParser : ILogLineParser
{
    private const string NcsaRegex = "^(?<Ip>\\S+) \\S+ (?<UserName>\\S+) \\[(?<Timestamp>[^\\]]+)\\] \"(?<Message>.+)\" (?<Status>[0-9]{3}) (?<Size>[0-9]+|-)";

    public Log Parse(string line, string appName, string serverName, string sourceName)
    {
        var match = Regex.Match(line, NcsaRegex);

        if (!match.Success)
        {
            throw new InvalidFormatException(line);
        }

        DateTimeOffset timestamp;
        var timestampStr = match.Groups["Timestamp"].Value;
        if (!DateTimeOffset.TryParseExact(timestampStr.Insert(timestampStr.Length - 2, ":"), "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
        {
            throw new InvalidFormatException(line);
        }

        var log = new Log
        {
            AppName = appName,
            Server = serverName,
            Source = sourceName,
            IP = match.Groups["Ip"].Value,
            Message = match.Groups["Message"].Value,
            HttpStatus = Int32.Parse(match.Groups["Status"].Value),
            LogEvent = $"{{\"Properties\":{{\"ResponseSize\":{match.Groups["Size"].Value}}}}}",
            TimeStamp = timestamp.ToUniversalTime()
        };

        var userName = match.Groups["UserName"].Value;
        if (userName != "-")
        {
            log.UserName = userName;
        }

        log.Level = log.HttpStatus >= 400 ? AggregatedLogsLevel.Error : AggregatedLogsLevel.Info;

        return log;
    }
}
