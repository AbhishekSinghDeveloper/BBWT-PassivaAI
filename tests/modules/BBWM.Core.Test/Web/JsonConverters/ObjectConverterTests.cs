using BBWM.Core.Utils;
using BBWM.Core.Web.JsonConverters;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BBWM.Core.Test.Web.JsonConverters
{
    public class ObjectConverterTests
    {
        [Fact]
        public void WriteTest()
        {
            object dummy = new DummyParent();
            var memoryStream = new MemoryStream();
            var utf8JsonWriter = new Utf8JsonWriter(memoryStream);
            var converter = new ObjectConverter();

            converter.Write(utf8JsonWriter, dummy, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);
            utf8JsonWriter.Flush();
            memoryStream.Position = 0;

            var streamReader = new StreamReader(memoryStream);
            var jsonText = streamReader.ReadToEnd().Replace(" ", string.Empty);
            streamReader.Dispose();

            Assert.Contains("\"child\":{", jsonText);
            Assert.Contains("\"stringField\":\"" + nameof(DummyChild) + "\"", jsonText);
            Assert.Contains("\"intField\":1", jsonText);
            Assert.Contains("\"trueField\":true", jsonText);
            Assert.Contains("\"falseField\":false", jsonText);
        }

        [Fact]
        public void ReadTest()
        {
            var json = "{\"child\":{\"stringField\":\"" + nameof(DummyChild) + "\", \"intField\":1, \"trueField\":true, \"falseField\":false, \"dateField\":\"2021-12-16T13:00:00Z\"}}";
            var utf8JsonReader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(json)));
            var converter = new ObjectConverter();

            var deserializedRule = converter.Read(ref utf8JsonReader, null, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);

            Assert.IsType<JsonElement>(deserializedRule);
        }

        [Fact]
        public void CanConvertTest()
        {
            var converter = new ObjectConverter();

            Assert.False(converter.CanConvert(typeof(DummyParent)));
            Assert.True(converter.CanConvert(typeof(object)));
        }


        private class DummyParent
        {
            public object Child { get; set; } = new DummyChild();
        }

        private class DummyChild
        {
            public object StringField { get; set; } = nameof(DummyChild);
            public object IntField { get; set; } = 1;
            public object DateField { get; set; } = DateTime.UtcNow;
            public object TrueField { get; set; } = true;
            public object FalseField { get; set; } = false;
        }
    }
}
