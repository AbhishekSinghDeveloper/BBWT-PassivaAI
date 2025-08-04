using System.Text.Json;

namespace BBWM.Core.Utils
{
    public static class JsonSerializerOptionsProvider
    {
        private static readonly JsonSerializerOptions _defaultOptions =
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        private static JsonSerializerOptions _options;
        private static JsonSerializerOptions _optionsWithoutCustomConverters;


        public static JsonSerializerOptions Options
        {
            get { return _options ?? _defaultOptions; }
            set { _options = value; }
        }

        public static JsonSerializerOptions OptionsWithoutCustomConverters
        {
            get { return _optionsWithoutCustomConverters ?? _defaultOptions; }
            set { _optionsWithoutCustomConverters = value; }
        }
    }
}
