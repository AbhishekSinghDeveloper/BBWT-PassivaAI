using BBWM.AggregatedLogs.Lambda.DTO;
using BBWM.Core.Loggers;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace BBWM.AggregatedLogs.Lambda
{
    public class AmazonLambdaLogParser : ILogParser
    {
        public async Task<IEnumerable<Log>> Parse(EventDTO logEvent, CancellationToken ct = default)
        {
            var data = await Decompress(logEvent.awslogs.data, ct);

            var logData = JsonSerializer.Deserialize<AwsLogDataDTO>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var logs = new List<Log>();
            foreach (var log in logData.LogEvents)
            {
                logs.Add(new Log
                {
                    AppName = logData.LogGroup,
                    Message = log.ExtractedFields.Event,
                    Source = AggregatedLogsSource.CloudWatch,
                    TimeStamp = DateTimeOffset.Parse(log.ExtractedFields.Timestamp),
                    LogEvent = $"{{\"Properties\":{{\"RequestId\":{log.ExtractedFields.RequestId}}}}}",
                    Level = MapType(log.ExtractedFields.Type)
                });
            }
            return logs;
        }

        private async Task<string> Decompress(string data, CancellationToken ct = default)
        {
            await using var input = new MemoryStream(Convert.FromBase64String(data));
            await using var output = new MemoryStream();

            await using var decompressStream = new GZipStream(input, CompressionMode.Decompress);
            await decompressStream.CopyToAsync(output);

            var result = output.ToArray();
            return Encoding.UTF8.GetString(result);
        }

        private string MapType(string type) => (type?.ToLower()) switch
        {
            "fail" => AggregatedLogsLevel.Error,
            "info" => AggregatedLogsLevel.Info,
            "warn" => AggregatedLogsLevel.Warning,
            "crit" => AggregatedLogsLevel.Critical,
            "trce" => AggregatedLogsLevel.Trace,
            "dbug" => AggregatedLogsLevel.Debug,
            _ => null,
        };
    }
}
