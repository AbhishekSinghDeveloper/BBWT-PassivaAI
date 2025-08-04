using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.Core.Web.JsonConverters
{
    public class JObjectConverter : JsonConverter<Newtonsoft.Json.Linq.JObject>
    {
        public override Newtonsoft.Json.Linq.JObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var document = JsonDocument.ParseValue(ref reader))
            {
                return Newtonsoft.Json.Linq.JObject.Parse(document.RootElement.ToString());
            }
        }

        public override void Write(Utf8JsonWriter writer, Newtonsoft.Json.Linq.JObject value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, JsonDocument.Parse(value.ToString()), options);
    }
}
