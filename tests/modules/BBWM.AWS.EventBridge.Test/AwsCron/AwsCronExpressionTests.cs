using BBWM.AWS.EventBridge.AwsCron;

using Xunit;

namespace BBWM.AWS.EventBridge.Test.AwsCron;

public class AwsCronExpressionTests
{
    [Theory]
    [MemberData(
        nameof(AwsCronExpressionTestsData.NextShouldBeTestData),
        MemberType = typeof(AwsCronExpressionTestsData))]
    public void NextOccurrence_ShouldBe(string awsCron, string shouldBe)
    {
        // Arrange
        var expr = AwsCronExpression.Parse(awsCron);

        // Act
        var next = expr.GetNextOccurrence(AwsCronExpressionTestsData.BaseDateTime);

        // Assert
        Assert.NotNull(next);
        Assert.Equal(shouldBe, next.Value.ToString("R"));
    }

    [Theory]
    [MemberData(
        nameof(AwsCronExpressionTestsData.NextShouldBeNullTestData),
        MemberType = typeof(AwsCronExpressionTestsData))]
    public void NextOccurrence_ShouldBeNull(string awsCron)
    {
        // Arrange
        var expr = AwsCronExpression.Parse(awsCron);

        // Act
        var next = expr.GetNextOccurrence(AwsCronExpressionTestsData.BaseDateTime);

        // Assert
        Assert.Null(next);
    }
}
