using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using BBF.Reporting.Core.Model.Variables;
using BBWM.Core.Exceptions;

namespace BBF.Reporting.Core.ModelJsonConverters;

public class EmittedVariableJsonConverter : JsonConverter<EmittedVariable>
{
    public override void Write(Utf8JsonWriter writer, EmittedVariable value, JsonSerializerOptions options) => throw new NotImplementedException();

    public override EmittedVariable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var jsonElement = document.RootElement;

        var variableTypeName = jsonElement.TryGetProperty("$type", out var typeJsonElement) ? typeJsonElement.GetString() : null;

        if (string.IsNullOrEmpty(variableTypeName))
            throw new InvalidModelException("An emitted variable must contains a type to determine to which class it should be converted.");

        var variableType = Assembly.GetAssembly(typeof(EmittedVariable))?.GetTypes().FirstOrDefault(type =>
            type.GetTypeInfo().IsSubclassOf(typeof(EmittedVariable)) && !type.IsAbstract &&
            string.Equals(type.Name, $"Emitted{variableTypeName}Variable", StringComparison.InvariantCultureIgnoreCase));

        if (variableType == null)
            throw new InvalidModelException($"A variable class with name 'Emitted{variableTypeName}Variable' does not exist.");

        return (EmittedVariable?)jsonElement.Deserialize(variableType, options);
    }

    public override bool CanConvert(Type objectType) => typeof(EmittedVariable).IsAssignableFrom(objectType);
}