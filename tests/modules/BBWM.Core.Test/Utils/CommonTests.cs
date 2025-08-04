using BBWM.Core.Utils;

using Moq;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Test.Utils;

public class CommonTests
{
    [Fact]
    public void Should_GetTypesWithAttribute()
    {
        // Arrange
        var assembly = GetAssembly();

        // Act
        var types = Common.GetTypesWithAttribute<ChildAttribute>(assembly);

        // Assert
        AssertIsChild(types);
    }

    [Fact]
    public void Should_GetTypesInheritedFrom()
    {
        // Arrange
        var assembly = GetAssembly();

        // Act
        var types = Common.GetTypesInheritedFrom<FakeClass>(assembly);

        // Assert
        Assert.All(
            new[] { typeof(FakeClass), typeof(FakeChildClass) },
            expected => Assert.Contains(types, actual => actual == expected));
    }

    [Fact]
    public void Should_GetTypesStrictlyInheritedFrom()
    {
        // Arrange
        var assembly = GetAssembly();

        // Act
        var types = Common.GetTypesStrictlyInheritedFrom<FakeClass>(assembly);

        // Assert
        AssertIsChild(types);
    }

    private static void AssertIsChild(IEnumerable<Type> types)
    {
        var actual = Assert.Single(types);
        Assert.Equal(typeof(FakeChildClass), actual);
    }

    private static Assembly GetAssembly()
    {
        var assembly = new Mock<Assembly>();
        assembly
            .Setup(p => p.GetTypes())
            .Returns(new[]
            {
                    typeof(FakeClass),
                    typeof(FakeChildClass),
            });

        return assembly.Object;
    }

    private class FakeClass
    {
        public string Int { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    [Child]
    private class FakeChildClass : FakeClass
    {
        public int Age { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    private class ChildAttribute : Attribute
    {

    }
}
