using BBWM.Core.Web;

using Xunit;

namespace BBWM.Core.Test.Web;

public class RouteTests
{
    private const string ParentPath = "parent";
    private const string ChildPath = "child";

    public RouteTests()
    {
    }

    private static RouteBuilder CreateRoute()
    {
        var parent = new RouteBuilder(ParentPath, null);

        return new RouteBuilder(ChildPath, parent);
    }

    [Fact]
    public void Test_Method_1()
    {
        // Arrange
        var service = CreateRoute();
        const string path = "fake";
        const string title = "Fake Title";

        // Act
        var route = service.Build(path, title);

        // Assert
        Assert.NotNull(route);
        Assert.Equal($"/{ParentPath}/{ChildPath}/{path}", route.Path);
        Assert.Equal(title, route.Title);
    }
}
