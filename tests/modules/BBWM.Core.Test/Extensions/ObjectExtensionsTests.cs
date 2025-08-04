
using Bogus;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Xunit;

using BbwtObjectExtensions = BBWM.Core.Extensions.ObjectExtensions;

namespace BBWM.Core.Test.Extensions;

public class ObjectExtensionsTests
{
    public static IEnumerable<object[]> DelegateTestData => new[]
    {
            new object[] { new Action(() => Console.WriteLine("Dummy output!")) },
            new object[] { new Func<int>(() => 10) },
        };

    public static IEnumerable<object[]> ArrayOfPrimitivesTestData => new[]
    {
            new object[] { new[] { 1, 2, 3 } },
            new object[] { new[] { 1.5, 2.5, 3.5 } },
            new object[] { new[] { 1f, 2f, 3f } },
            new object[] { new[] { "hello", "dummy", "string" } },
        };

    [Fact]
    public void Should_DeepCopy_Null_Object()
    {
        // Arrange & Act
        var copy = BbwtObjectExtensions.DeepCopy(null);

        // Assert
        Assert.Null(copy);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(2.5, 2.5)]
    [InlineData(6f, 6f)]
    [InlineData("dummy", "dummy")]
    public void Should_DeepCopy_Primitives(object original, object shouldBe)
    {
        // Arrange & Act
        var copy = BbwtObjectExtensions.DeepCopy(original);

        // Assert
        AssertPrimitive(shouldBe, copy);
    }

    [Fact]
    public void Should_DeepCopy_Object()
    {
        // Arrange
        A a = new()
        {
            D = "A",
            B = new()
            {
                D = "B",
                A = new() { D = "A2" },
            },
        };

        // Act
        A a2 = BbwtObjectExtensions.DeepCopy(a);

        // Assert
        AssertObject(a, a2, a);
        AssertObject(a.B, a2.B, a.B);
        AssertObject(a.B.A, a2.B.A, a);
    }

    [Fact]
    public void Should_DeepCopy_Circular_References()
    {
        // Arrange
        A a = new() { D = "A" };
        a.B = new B { A = a, D = "B" };

        // Act
        A a2 = BbwtObjectExtensions.DeepCopy(a);

        // Assert
        AssertObject(a, a2, a);
        AssertObject(a.B, a2.B, a.B);
        Assert.NotSame(a2.B.A, a);
    }

    [Theory]
    [MemberData(nameof(DelegateTestData))]
    public void Should_Not_DeepCopy_Delegate(object @delegate)
    {
        // Arrange & Act
        var copy = BbwtObjectExtensions.DeepCopy(@delegate);

        // Assert
        Assert.Null(copy);
    }

    [Theory]
    [MemberData(nameof(ArrayOfPrimitivesTestData))]
    public void Should_DeepCopy_Array_of_Primitives(object array)
    {
        // Arrange & Act
        var copy = BbwtObjectExtensions.DeepCopy(array);

        // Assert
        AssertArrays(array, copy, AssertPrimitive);
    }

    [Fact]
    public void Should_DeepCopy_Array_of_Objects()
    {
        // Arrange
        var index = 1;
        var array = new Faker<A>()
            .RuleFor(a => a.D, _ => $"A{index}")
            .RuleFor(a => a.B, _ => new() { D = $"B{index++}" })
            .Generate(20)
            .ToArray();

        // Act
        var copy = BbwtObjectExtensions.DeepCopy(array);

        // Assert
        AssertArrays(array, copy, (e, a) =>
        {
            var (expected, actual) = (e as A, a as A);
            AssertObject(expected, actual, expected);
            AssertObject(expected.B, actual.B, expected.B);
        });
    }

    private static void AssertPrimitive(object expected, object actual)
    {
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected, actual);
    }

    private static void AssertArrays(object expected, object actual, Action<object, object> assertItem)
    {
        Assert.NotSame(expected, actual);
        Assert.IsType(expected.GetType(), actual);

        var (expectedArray, actualArray) = (expected as Array, actual as Array);
        Assert.Equal(expectedArray.Length, actualArray.Length);

        var zipped = Zip(expectedArray, actualArray).ToList();
        Assert.All(zipped, z => assertItem(z.Expected, z.Actual));
    }

    private static void AssertObject<T>(T expected, T actual, IEqualityComparer<T> comparer)
        where T : class
    {
        Assert.NotSame(expected, actual);
        Assert.Equal(expected, actual, comparer);
    }

    private static IEnumerable<Zipped> Zip(IEnumerable expected, IEnumerable actual)
    {
        var expectedIE = expected.GetEnumerator();
        var actualIE = actual.GetEnumerator();

        while (expectedIE.MoveNext() && actualIE.MoveNext())
        {
            yield return new Zipped
            {
                Expected = expectedIE.Current,
                Actual = actualIE.Current,
            };
        }
    }

    private class A : IEqualityComparer<A>
    {
        public string D { get; set; }

        public B B { get; set; }

        public bool Equals(A x, A y)
            => string.Compare(x.D, y.D, false, CultureInfo.InvariantCulture) == 0;

        public int GetHashCode([DisallowNull] A obj) => $"A:{D}".GetHashCode();
    }

    private class B : IEqualityComparer<B>
    {
        public string D { get; set; }

        public A A { get; set; }

        public bool Equals(B x, B y) => string.Compare(x.D, y.D, false, CultureInfo.InvariantCulture) == 0;

        public int GetHashCode([DisallowNull] B obj) => $"B:{D}".GetHashCode();
    }

    private class Zipped
    {
        public object Expected { get; set; }

        public object Actual { get; set; }
    }
}
