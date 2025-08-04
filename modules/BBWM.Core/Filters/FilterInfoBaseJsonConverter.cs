using BBWM.Core.Exceptions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.Core.Filters;

public class FilterInfoBaseJsonConverter : JsonConverter<FilterInfoBase>
{
    public override void Write(Utf8JsonWriter writer, FilterInfoBase value, JsonSerializerOptions options) => throw new NotImplementedException();

    public override FilterInfoBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var document = JsonDocument.ParseValue(ref reader))
        {
            var jsonElement = document.RootElement;

            string filterTypeName = null;
            if (jsonElement.TryGetProperty("$type", out var typeJsonElement))
                filterTypeName = typeJsonElement.GetString();
            else
                throw new InvalidModelException("An instance of the 'FilterInfoBase' class must contains the '$type' property to determine which class it should be cast to.");

            if (string.IsNullOrWhiteSpace(filterTypeName))
                throw new InvalidModelException("The '$type' property of an instance of the 'FilterInfoBase' class can't be empty.");

            var filterType = Assembly.GetAssembly(typeof(FilterInfoBase))
                .GetTypes()
                .FirstOrDefault(a => a.GetTypeInfo()
                    .IsSubclassOf(
                        typeof(FilterInfoBase)) &&
                        !a.IsAbstract &&
                        string.Equals(a.Name, $"{filterTypeName}Filter", StringComparison.InvariantCultureIgnoreCase)
                    );

            if (filterType == null)
                throw new InvalidModelException($"A filter class with name '{filterTypeName}Filter' does not exist.");

            return (FilterInfoBase)jsonElement.Deserialize(filterType, options);
        }
    }

    public override bool CanConvert(Type objectType) => typeof(FilterInfoBase).IsAssignableFrom(objectType);
}
