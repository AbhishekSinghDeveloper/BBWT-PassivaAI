using BBWM.Core.Filters.Handlers;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Test.Filters.Handlers;
using System.Reflection;

using Xunit;

namespace BBWM.Core.Test.Filters.Handlers;

public class IsNullFilterHandlerTests
{
    [Theory]
    [MemberData(nameof(Data))]
    public void Handle_Should_Handle_IsNull_Filter(string propertyName, MyEntityHelper value, bool shouldBe)
    {
        // Arrange
        var handler = CreateHandler(propertyName);

        // Act
        var expr = handler.Handle<MyEntityHelper>();

        // Assert
        expr.AssertHandlerExpression(value, shouldBe);
    }

    [Fact]
    public void Handle_Should_Throw_Exception()
    {
        // Arrange
        var handler = CreateHandler(nameof(MyEntityHelper.MyMaxProperty));

        // Act & Assert
        Assert.Throws<Exception>(handler.Handle<MyEntityHelper>);
    }

    public static IEnumerable<object[]> Data => new[]
    {
            new object[]
            {
                nameof(MyEntityHelper.MyString),
                new MyEntityHelper(),
                true,
            },
            new object[]
            {
                nameof(MyEntityHelper.MyString),
                new MyEntityHelper { MyString = string.Empty },
                false,
            },
            new object[]
            {
                nameof(MyEntityHelper.MyString),
                new MyEntityHelper { MyString = "Dummy String" },
                false,
            },
            new object[]
            {
                nameof(MyEntityHelper.MyNullableInteger),
                new MyEntityHelper(),
                true,
            },
            new object[]
            {
                nameof(MyEntityHelper.MyNullableInteger),
                new MyEntityHelper { MyNullableInteger = 10 },
                false,
            },
            new object[]
            {
                nameof(MyEntityHelper.MyDTO),
                new MyEntityHelper(),
                true,
            },
            new object[]
            {
                nameof(MyEntityHelper.MyDTO),
                new MyEntityHelper { MyDTO = new MyDTOHelper() },
                false,
            },
        };

    private static FilterHandlerBase CreateHandler(string propertyName)
    {
        var nullFilter = new IsNullFilter { PropertyName = propertyName };
        var relatedAttr = typeof(IsNullFilter).GetCustomAttribute<RelatedHandlerAttribute>();
        return Activator.CreateInstance(relatedAttr.HandlerType, nullFilter) as FilterHandlerBase;
    }
}
