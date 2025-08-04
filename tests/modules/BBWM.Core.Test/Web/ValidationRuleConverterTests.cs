using BBWM.Core.Membership.DTO;
using BBWM.Core.Utils;
using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.Web;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BBWM.Core.Test.Web;

public class ValidationRuleConverterTests
{
    [Fact]
    public void WriteTest()
    {
        var requiredValidationRule = new RequiredValidationRule();
        var memoryStream = new MemoryStream();
        var utf8JsonWriter = new Utf8JsonWriter(memoryStream);
        var converter = new ValidationRuleConverter();

        converter.Write(utf8JsonWriter, requiredValidationRule, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);
        utf8JsonWriter.Flush();
        memoryStream.Position = 0;

        var streamReader = new StreamReader(memoryStream);
        var jsonText = streamReader.ReadToEnd().Replace(" ", string.Empty);
        streamReader.Dispose();

        Assert.Contains("\"$type\":\"required\"", jsonText);
    }

    [Fact]
    public void ReadTest()
    {
        var json = "{\"$type\":\"required\"}";
        var utf8JsonReader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(json)));
        var converter = new ValidationRuleConverter();

        var deserializedRule = converter.Read(ref utf8JsonReader, null, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);

        Assert.IsType<RequiredValidationRule>(deserializedRule);
    }

    [Fact]
    public void CanConvertTest()
    {
        var converter = new ValidationRuleConverter();

        Assert.False(converter.CanConvert(typeof(UserDTO)));
        Assert.False(converter.CanConvert(typeof(AbstractRule)));
        Assert.True(converter.CanConvert(typeof(RequiredValidationRule)));
        Assert.True(converter.CanConvert(typeof(NumberRangeValidationRule)));
        Assert.True(converter.CanConvert(typeof(DateRangeValidationRule)));
        Assert.True(converter.CanConvert(typeof(InputFormatValidationRule)));
        Assert.True(converter.CanConvert(typeof(MaxLengthValidationRule)));
    }


    private abstract class AbstractRule : ValidationRule
    {
    }
}
