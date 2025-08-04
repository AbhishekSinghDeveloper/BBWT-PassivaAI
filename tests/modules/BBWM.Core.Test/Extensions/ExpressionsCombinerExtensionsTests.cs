using BBWM.Core.Extensions;

using System.Linq.Expressions;

using Xunit;

namespace BBWM.Core.Test.Extensions;

public class ExpressionsCombinerExtensionsTests
{
    public ExpressionsCombinerExtensionsTests()
    {
    }

    [Fact]
    public void Or_State_Under_Test()
    {
        Expression<Func<FakeClass, bool>> param = x => x.FirstName == "TestName";
        Expression<Func<FakeClass, bool>> param2 = x => x.FirstName == "TestName";

        var combiner = ExpressionsCombinerExtensions.Or<FakeClass>(param, param2);

        Assert.NotNull(combiner);
        Assert.NotNull(param);
        Assert.NotNull(param2);
    }

    [Fact]
    public void And_State_Under_Test()
    {
        Expression<Func<FakeClass, bool>> param = x => x.FirstName == "TestName";
        Expression<Func<FakeClass, bool>> param2 = x => x.FirstName == "TestName";

        var combiner = ExpressionsCombinerExtensions.And<FakeClass>(param, param2);

        Assert.NotNull(combiner);
        Assert.NotNull(param);
        Assert.NotNull(param2);
    }

    private class FakeClass
    {
        public string FirstName { get; set; }

        public FakeClass() { }
    }
}
