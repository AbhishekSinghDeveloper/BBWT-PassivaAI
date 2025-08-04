using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.Core.Loggers.VictoriaLogs
{
    public class VictoriaLogsSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly string _projectName;

        private static HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("http://localhost:9428/insert/"),
        };

        private static JsonSerializerOptions opts = new JsonSerializerOptions();
        

        public VictoriaLogsSink(IFormatProvider formatProvider, string projectName)
        {
            _formatProvider = formatProvider;
            _projectName = projectName;

            if (!opts.Converters.Any(x => x.GetType() == typeof(JsonStringEnumConverter)))
            {
                var stringEnumConverter = new JsonStringEnumConverter();
                opts.Converters.Add(stringEnumConverter);
            }
        }

        public void Emit(LogEvent logEvent)
        {
            var task = PostAsJsonAsync(sharedClient, logEvent, _projectName);
            task.RunSynchronously();

            //var message = logEvent.RenderMessage(_formatProvider);
            //Console.WriteLine(DateTimeOffset.Now.ToString() + " " + message);
        }

        async Task PostAsJsonAsync(HttpClient httpClient, LogEvent logEvent, string projectName)
        {
            var message = logEvent.RenderMessage(_formatProvider);

            VictoriaLogObject logObject = new VictoriaLogObject
            {
                LogEvent = logEvent,
                ProjectName = projectName,
                Properties = message//logEvent.Properties
            };

            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "jsonline",
                logObject,
                         options: opts);

            response.EnsureSuccessStatusCode();
        }
    }
}
