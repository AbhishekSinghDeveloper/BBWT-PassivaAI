using Serilog.Core;
using Serilog.Events;
using Newtonsoft.Json;
using System.Text;
using BBWM.Core.Loggers.QuickWit;
using Microsoft.AspNetCore.Hosting;

namespace BBWM.Core.Loggers.VictoriaLogs
{
    public class QuickWitSink : ILogEventSink
    {
        private readonly string _projectName;

        private readonly string postDataPath = "ingest?commit=force";

        private readonly string indexPath = "indexes/";

        private static HttpClient sharedClient;

        private static QuickWitOptions _options;

        private static bool _indexExists = false;

        public QuickWitSink(QuickWitOptions options, string projectName)
        {
            sharedClient = new HttpClient
            {
                BaseAddress = new Uri(options.Hostname)
            };

            _projectName = projectName;

            _options = options;

            // check index exists
            _indexExists = CheckIndexExistAsync().GetAwaiter().GetResult();
        }
        public void Emit(LogEvent logEvent)
        {
            if (!_indexExists)
            {
                return;
            }

            var task = PostAsJsonAsync(logEvent);

            task.RunSynchronously();
        }

        async Task<bool> CheckIndexExistAsync()
        {
            using HttpResponseMessage response = await sharedClient.GetAsync(
                $"{indexPath}{_projectName}");

            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }

        async Task PostAsJsonAsync(LogEvent logEvent)
        {
            var json = JsonConvert.SerializeObject(logEvent, _options.SerializerSettings);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            content.Headers.ContentLength = json.Length;

            await sharedClient.PostAsync($"{_projectName}/{postDataPath}", content);
        }
    }
}
