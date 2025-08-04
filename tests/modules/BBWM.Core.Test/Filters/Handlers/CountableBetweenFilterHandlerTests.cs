using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Filters.Handlers;
using BBWM.Core.Filters.TypedFilters;
using Xunit;

namespace BBWM.Core.Test.Filters.Handlers;

public class CountableBetweenFilterHandlerTests
{
    [Theory]
    [MemberData(nameof(DateBetweenFilterData))]
    public void Handle_Should_Handle_DateBetween_Filter(DateTime from, DateTime to, DateTime dt, bool shouldBe)
    {
        // Arrange
        var dateBetweenFilter = new DateBetweenFilter
        {
            From = from,
            To = to,
            PropertyName = nameof(EventBridgeJobHistory.StartTime),
        };
        var handler = new CountableBetweenFilterHandler<DateTime>(dateBetweenFilter);

        // Act
        var expr = handler.Handle<EventBridgeJobHistory>();

        // Assert
        expr.AssertHandlerExpression(new EventBridgeJobHistory { StartTime = dt }, shouldBe);
    }

    [Theory]
    [InlineData(1, 5, 2, true)]
    [InlineData(1, 5, 1.001, true)]
    [InlineData(1, 5, 4.0089, true)]
    [InlineData(1, 5, 5, true)]
    [InlineData(1, 5, 1, true)]
    [InlineData(1, 5, .99, false)]
    [InlineData(1, 5, 5.1, false)]
    public void Handle_Should_Handle_NumberBetween_Filter(double from, double to, double value, bool shouldBe)
    {
        // Arrange
        var numberFilter = new NumberBetweenFilter
        {
            From = from,
            To = to,
            PropertyName = nameof(MyEntityHelper.MyMaxProperty),
        };
        var handler = new CountableBetweenFilterHandler<double>(numberFilter);

        // Act
        var expr = handler.Handle<MyEntityHelper>();

        // Assert
        expr.AssertHandlerExpression(new MyEntityHelper { MyMaxProperty = value }, shouldBe);
    }

    public static IEnumerable<object[]> DateBetweenFilterData => new[]
    {
            new object[]
            {
                new DateTime(2021, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2021, 12, 14, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 13, 10, 0, 0, DateTimeKind.Utc),
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2021, 12, 14, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 14, 23, 59, 59, DateTimeKind.Utc),
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2021, 12, 14, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                true,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2021, 12, 14, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 11, 23, 59, 59, DateTimeKind.Utc),
                false,
            },
            new object[]
            {
                new DateTime(2021, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2021, 12, 14, 23, 59, 59, DateTimeKind.Utc),
                new DateTime(2021, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                false,
            },
        };
}
