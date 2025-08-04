using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.Core.Web.JsonConverters
{
    /// <summary>
    /// This class is required because there is no way to pass
    /// the non-constant JsonNamingPolicy argument into
    /// the base class constructor inside the JsonConverterAttribute.
    /// </summary>
    public class JsonStringEnumCamelCaseConverter : JsonStringEnumConverter
    {
        public JsonStringEnumCamelCaseConverter() : base(JsonNamingPolicy.CamelCase)
        {
        }
    }
}
