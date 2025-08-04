using BBWM.Core.Filters.Handlers;
using BBWM.Core.Filters.TypedFilters;
using Xunit;

namespace BBWM.Core.Test.Filters.Handlers;

public class BooleanFilterHandlerTests
{
    public BooleanFilterHandlerTests()
    {
    }

    [Fact]
    public void Handle_StateUnderTest_ExpectedBehavior()
    {
        var boolFilter = new BooleanFilter();
        boolFilter.Value = new FakeClass().IsWorking;
        boolFilter.PropertyName = "IsWorking";

        var boolFilter2 = new BooleanFilter();

        var boolFilterHandler = new BooleanFilterHandler(boolFilter);
        var boolFilterHandler2 = new BooleanFilterHandler(boolFilter2);

        boolFilterHandler.Handle<FakeClass>();
        boolFilterHandler2.Handle<FakeClass>();

        Assert.NotNull(boolFilter);
        Assert.NotNull(boolFilter2);
        Assert.NotNull(boolFilterHandler);
        Assert.NotNull(boolFilterHandler2);
    }



    private class FakeClass
    {
        public bool IsWorking { get; set; }

        public FakeClass() { }
    }
}
