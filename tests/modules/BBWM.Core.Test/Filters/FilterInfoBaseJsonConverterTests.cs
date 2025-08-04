using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Utils;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BBWM.Core.Test.Filters;

public class FilterInfoBaseJsonConverterTests
{
    [Fact]
    public void WriteJsonShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = new FilterInfoBaseJsonConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => converter.Write(default, default, default));
    }

    [Theory]
    [InlineData(FilterInfoBaseJsonConverterTestData.FailFilterHasNotTypeKey)]
    [InlineData(FilterInfoBaseJsonConverterTestData.FailTypeKeyIsNull)]
    [InlineData(FilterInfoBaseJsonConverterTestData.FailTypeKeyIsWhiteSpace)]
    [InlineData(FilterInfoBaseJsonConverterTestData.FailFilterTypeMissing)]
    public void ReadShouldThrowInvalidModelException(string json)
    {
        // Arrange
        var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(json)));
        var converter = new FilterInfoBaseJsonConverter();

        // Act & Assert
        try
        {
            converter.Read(ref reader, default, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);
        }
        catch (Exception ex)
        {
            Assert.IsType<InvalidModelException>(ex);
        }
    }

    // TODO: (2022-08-01) Uncomment and fix !!!
    //[Theory]
    //[MemberData(
    //    nameof(FilterInfoBaseJsonConverterTestData.FilterTestData),
    //    MemberType = typeof(FilterInfoBaseJsonConverterTestData))]
    //public void ReadShouldCreateFilter(string json, Action<object> assertFilter)
    //{
    //    // Arrange
    //    var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(json)));
    //    var converter = new FilterInfoBaseJsonConverter();

    //    // Act
    //    var filter = converter.Read(ref reader, default, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);

    //    // Assert
    //    Assert.NotNull(filter);
    //    assertFilter?.Invoke(filter);
    //}

    [Theory]
    [InlineData(typeof(StringFilter))]
    [InlineData(typeof(BooleanFilter))]
    [InlineData(typeof(NumberFilter))]
    [InlineData(typeof(NumberBetweenFilter))]
    [InlineData(typeof(DateFilter))]
    [InlineData(typeof(DateBetweenFilter))]
    [InlineData(typeof(IsNullFilter))]
    public void Should_CanConvert(Type filterType)
    {
        // Arrange
        var converter = new FilterInfoBaseJsonConverter();

        // Act
        var canConvert = converter.CanConvert(filterType);

        // Assert
        Assert.True(canConvert);
    }
}
