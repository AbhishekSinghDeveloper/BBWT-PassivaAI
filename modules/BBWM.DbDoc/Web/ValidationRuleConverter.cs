using BBWM.DbDoc.Core.Classes.ValidationRules;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.DbDoc.Web;

public class ValidationRuleConverter : JsonConverter<ValidationRule>
{
    public static readonly List<(string Alias, Type ValidatorType)> TypesMap = new List<(string, Type)>
        {
            ("required", typeof(RequiredValidationRule)),
            ("number_range", typeof(NumberRangeValidationRule)),
            ("date_range", typeof(DateRangeValidationRule)),
            ("input_format", typeof(InputFormatValidationRule)),
            ("max_length", typeof(MaxLengthValidationRule))
        };


    public override void Write(Utf8JsonWriter writer, ValidationRule value, JsonSerializerOptions options)
    {
        var valueType = value.GetType();

        var mapItem = TypesMap.FirstOrDefault(x => x.ValidatorType == valueType);
        if (mapItem.Equals(default((string, Type))))
            throw new Exception($"The validation rule's type '{valueType.Name}' is not supported.");

        var jsonObj = JsonSerializer.SerializeToNode(value, valueType, options);
        jsonObj["$type"] = mapItem.Alias;

        jsonObj.WriteTo(writer);
    }

    public override ValidationRule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var document = JsonDocument.ParseValue(ref reader))
        {
            var jsonObject = document.RootElement;
            var alias = jsonObject.GetProperty("$type").GetString();
            var mapItem = TypesMap.FirstOrDefault(x => string.Equals(x.Alias, alias, StringComparison.InvariantCultureIgnoreCase));

            if (mapItem.Equals(default((string, Type))))
                throw new Exception($"The validation rule's type '{alias}' is not supported.");

            return (ValidationRule)jsonObject.Deserialize(mapItem.ValidatorType, options);
        }
    }

    public override bool CanConvert(Type objectType)
        => objectType == typeof(ValidationRule) || !objectType.IsAbstract && objectType.IsSubclassOf(typeof(ValidationRule));
}
