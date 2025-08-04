using BBWM.Core.Extensions;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Extensions;

public class TypeExtensionsTests
{
    public TypeExtensionsTests()
    {
    }

    [Fact]
    public void Is_Sub_Class_Of_Generic_Test()
    {
        var mockType = new Mock<Type>();
        var mockType2 = new Mock<Type>();
        mockType2.Setup(p => p.IsGenericType).Returns(true);

        var subClassOfGeneric = TypeExtensions.IsSubClassOfGeneric(mockType.Object, mockType2.Object);
        Action isGenericIsFalse = () => TypeExtensions.IsSubClassOfGeneric(mockType.Object, mockType.Object);

        Assert.NotNull(subClassOfGeneric);
        Assert.Throws<Exception>(isGenericIsFalse);
    }
}
