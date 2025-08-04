using BBWM.Core.Utils;

using Moq;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Test.Utils;

public class ReflectionHelperTests
{
    [Fact]
    public void Should_GetAllConstantsValuesFromClassesOfAssembly()
    {
        // Arrange
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(ConstantsHelper) });

        // Act
        var constants =
            ReflectionHelper.GetAllConstantsValuesFromClassesOfAssembly<string>(
                assembly.Object, nameof(ConstantsHelper));

        // Assert
        AssertConstants(new[] { ConstantsHelper.MyConstant1, ConstantsHelper.MyConstant2 }, constants);
    }

    [Fact]
    public void Should_GetAllConstantsValuesOfClass()
    {
        // Arrange & Act
        var constants = ReflectionHelper.GetAllConstantsValuesOfClass<int>(typeof(ConstantsHelper));

        // Assert
        AssertConstants(new[] { ConstantsHelper.MyConstant3, ConstantsHelper.MyConstant3 }, constants);
    }

    private static void AssertConstants<T>(IEnumerable<T> expectedConstants, IEnumerable<T> actualConstants)
        where T : IEquatable<T>
        => Assert.All(
            expectedConstants,
            expected => Assert.Contains(actualConstants, actual => expected.Equals(actual)));

    private class ConstantsHelper
    {
        public const string MyConstant1 = "Hello";

        public const string MyConstant2 = "World!";

        public const int MyConstant3 = 3;

        public const int MyConstant4 = 5;
    }
}
