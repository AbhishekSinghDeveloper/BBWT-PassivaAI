using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using Xunit;

namespace BBWM.Core.Test.Filters;

public static class FilterInfoBaseJsonConverterTestData
{
    public const string FailFilterHasNotTypeKey = "{ }";

    public const string FailTypeKeyIsNull = "{ \"$type\": null }";

    public const string FailTypeKeyIsWhiteSpace = "{ \"$type\": \"    \" }";

    public const string FailFilterTypeMissing = "{ \"$type\": \"Dummy-007\" }";

    public const string StringFilter = @"{
            ""id"": null,
            ""$type"": ""string"",
            ""propertyName"": ""code"",
            ""value"": ""asd"",
            ""matchMode"": 0
        }";

    public const string BooleanFilter = @"{
            ""id"": null,
            ""$type"": ""boolean"",
            ""propertyName"": ""isPaid"",
            ""value"": true
        }";

    public const string NumberFilter = @"{
            ""id"": null,
            ""$type"": ""number"",
            ""propertyName"": ""id"",
            ""value"": 3,
            ""matchMode"": 0
        }";

    public const string NumberBetweenFilter = @"{
            ""id"": null,
            ""$type"": ""numberBetween"",
            ""propertyName"": ""id"",
            ""from"": 2,
            ""to"": 5
        }";

    public const string DateFilter = @"{
            ""id"": null,
            ""$type"": ""date"",
            ""propertyName"": ""requiredDate"",
            ""value"": ""2021-12-16T13:00:00Z"",
            ""matchMode"": 0
        }";

    public const string DateBetweenFilter = @"{
            ""id"": null,
            ""$type"": ""dateBetween"",
            ""propertyName"": ""shippedDate"",
            ""from"": ""2021-12-09T13:00:00Z"",
            ""to"": ""2021-12-16T13:00:00Z""
        }";

    public const string IsNullFilter = @"{
            ""id"": null,
            ""$type"": ""isNull"",
            ""propertyName"": ""authSecurityStamp""
        }";

    public static IEnumerable<object[]> FilterTestData
        => new string[]
        {
            StringFilter,
            BooleanFilter,
            NumberFilter,
            NumberBetweenFilter,
            DateFilter,
            DateBetweenFilter,
            IsNullFilter
        }
        .Zip(filterAsserts)
        .Select(zipped => new object[] { zipped.First, zipped.Second });

    private static Action<object>[] filterAsserts =
    {
           AssertStringFilter,
           AssertBooleanFilter,
           AssertNumberFilter,
           AssertNumberBetweenFilter,
           AssertDateFilter,
           AssertDateBetweenFilter,
           AssertIsNullFilter,
    };

    private static void AssertStringFilter(object filter)
    {
        var stringFilter = Assert.IsAssignableFrom<StringFilter>(filter);
        Assert.Equal("code", stringFilter.PropertyName);
        Assert.Equal("asd", stringFilter.Value);
        Assert.Equal(StringFilterMatchMode.Contains, stringFilter.MatchMode);
    }

    private static void AssertBooleanFilter(object filter)
    {
        var booleanFilter = Assert.IsAssignableFrom<BooleanFilter>(filter);
        Assert.Equal("isPaid", booleanFilter.PropertyName);
        Assert.True(booleanFilter.Value);
    }

    private static void AssertNumberFilter(object filter)
    {
        var numberFilter = Assert.IsAssignableFrom<NumberFilter>(filter);
        Assert.Equal("id", numberFilter.PropertyName);
        Assert.Equal(3, numberFilter.Value);
        Assert.Equal(CountableFilterMatchMode.Equals, numberFilter.MatchMode);
    }

    private static void AssertNumberBetweenFilter(object filter)
    {
        var numberBetweenFilter = Assert.IsAssignableFrom<NumberBetweenFilter>(filter);
        Assert.Equal("id", numberBetweenFilter.PropertyName);
        Assert.Equal(2, numberBetweenFilter.From);
        Assert.Equal(5, numberBetweenFilter.To);
    }

    private static void AssertDateFilter(object filter)
    {
        var dateFilter = Assert.IsAssignableFrom<DateFilter>(filter);
        Assert.Equal("requiredDate", dateFilter.PropertyName);
        Assert.Equal("20211216", $"{dateFilter.Value:yyyyMMdd}");
        Assert.Equal(CountableFilterMatchMode.Equals, dateFilter.MatchMode);
    }

    private static void AssertDateBetweenFilter(object filter)
    {
        var dateBetweenFilter = Assert.IsAssignableFrom<DateBetweenFilter>(filter);
        Assert.Equal("shippedDate", dateBetweenFilter.PropertyName);
        Assert.Equal("20211209", $"{dateBetweenFilter.From:yyyyMMdd}");
        Assert.Equal("20211216", $"{dateBetweenFilter.To:yyyyMMdd}");
    }

    private static void AssertIsNullFilter(object filter)
    {
        var isNullFilter = Assert.IsAssignableFrom<IsNullFilter>(filter);
        Assert.Equal("authSecurityStamp", isNullFilter.PropertyName);
    }
}