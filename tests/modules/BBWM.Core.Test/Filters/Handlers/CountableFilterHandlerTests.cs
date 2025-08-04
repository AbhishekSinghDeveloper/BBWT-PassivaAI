using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Filters;
using BBWM.Core.Filters.Handlers;
using BBWM.Core.Filters.TypedFilters;
using Xunit;

namespace BBWM.Core.Test.Filters.Handlers;

public class CountableFilterHandlerTests
{
    [Theory]
    [MemberData(nameof(DateFilterData))]
    public void Handle_Should_Handle_Date_Filter(
        DateTime filterValue, DateTime value, CountableFilterMatchMode matchMode, bool shouldBe)
    {
        // Arrange
        var dateFilter = new DateFilter
        {
            Value = filterValue,
            MatchMode = matchMode,
            PropertyName = nameof(EventBridgeJobHistory.StartTime),
        };
        var handler = new DateFilterHandler(dateFilter);

        // Act
        var expr = handler.Handle<EventBridgeJobHistory>();

        // Assert
        expr.AssertHandlerExpression(new EventBridgeJobHistory { StartTime = value }, shouldBe);
    }

    [Theory]
    [MemberData(nameof(NumberFilterData))]
    public void Handle_Should_Handle_Number_Filter(
        double filterValue, double value, CountableFilterMatchMode matchMode, bool shouldBe)
    {
        // Arrange
        var numberFilter = new NumberFilter
        {
            Value = filterValue,
            MatchMode = matchMode,
            PropertyName = nameof(MyEntityHelper.MyMaxProperty),
        };
        var handler = new NumberFilterHandler(numberFilter);

        // Act
        var expr = handler.Handle<MyEntityHelper>();

        // Assert
        expr.AssertHandlerExpression(new MyEntityHelper { MyMaxProperty = value }, shouldBe);
    }

    public static IEnumerable<object[]> DateFilterData => new[]
    {
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 58, DateTimeKind.Utc),
                CountableFilterMatchMode.LessThan,
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                CountableFilterMatchMode.LessThan,
                false,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                CountableFilterMatchMode.LessThanOrEqual,
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 13, 0, 0, 0, DateTimeKind.Utc),
                CountableFilterMatchMode.LessThanOrEqual,
                false,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                CountableFilterMatchMode.GreaterThanOrEqual,
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 58, DateTimeKind.Utc),
                CountableFilterMatchMode.GreaterThanOrEqual,
                false,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 13, 0, 0, 0, DateTimeKind.Utc),
                CountableFilterMatchMode.GreaterThan,
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                CountableFilterMatchMode.GreaterThan,
                false,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 58, DateTimeKind.Utc),
                CountableFilterMatchMode.NotEquals,
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                CountableFilterMatchMode.NotEquals,
                false,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                CountableFilterMatchMode.Equals,
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 23, 59, 58, DateTimeKind.Utc),
                CountableFilterMatchMode.Equals,
                false,
            },
        };

    public static IEnumerable<object[]> NumberFilterData => new[]
    {
            new object[] { 10, 9.9, CountableFilterMatchMode.LessThan, true },
            new object[] { 10, 10, CountableFilterMatchMode.LessThan, false },
            new object[] { 10, 10, CountableFilterMatchMode.LessThanOrEqual, true },
            new object[] { 10, 10.02, CountableFilterMatchMode.LessThanOrEqual, false },
            new object[] { 10, 10, CountableFilterMatchMode.GreaterThanOrEqual, true },
            new object[] { 10, 9.08, CountableFilterMatchMode.GreaterThanOrEqual, false },
            new object[] { 10, 10.02, CountableFilterMatchMode.GreaterThan, true },
            new object[] { 10, 9.08, CountableFilterMatchMode.GreaterThan, false },
            new object[] { 10, 10.1, CountableFilterMatchMode.NotEquals, true },
            new object[] { 10, 10, CountableFilterMatchMode.NotEquals, false },
            new object[] { 10, 10, CountableFilterMatchMode.Equals, true },
            new object[] { 10, 9.02, CountableFilterMatchMode.Equals, false },
        };
}
