using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Sinks.Graylog.Core.Helpers;
using Serilog.Sinks.Graylog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBWM.Core.Loggers.QuickWit
{
    public class QuickWitOptions
    {
        public static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.None,
            Converters = new List<JsonConverter>()
        };

        public LogEventLevel MinimumLogEventLevel { get; set; }

        public string Hostname { get; set; }

        
        public JsonSerializerSettings SerializerSettings { get; set; }

        public QuickWitOptions()
        {
            MinimumLogEventLevel = LogEventLevel.Verbose;
            SerializerSettings = DefaultSerializerSettings;
        }
    }
}
