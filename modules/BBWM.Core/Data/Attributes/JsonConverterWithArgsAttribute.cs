using System.Text.Json.Serialization;

namespace BBWM.Core.Data.Attributes
{
    public class JsonConverterWithArgsAttribute : JsonConverterAttribute
    {
        public JsonConverterWithArgsAttribute(Type converterType, params object[] converterArguments)
        {
            ConverterType = converterType;
            ConverterArguments = converterArguments;
        }


        // CreateConverter method is only called if base.ConverterType is null 
        // so store the type parameter in a new property in the derived attribute
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.Converters.cs#L278
        public new Type ConverterType { get; }

        public object[] ConverterArguments { get; }


        public override JsonConverter CreateConverter(Type _) =>
            (JsonConverter)Activator.CreateInstance(ConverterType, ConverterArguments);
    }
}
