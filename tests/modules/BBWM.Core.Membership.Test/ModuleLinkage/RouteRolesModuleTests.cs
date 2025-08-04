using BBWM.Core.Membership.ModuleLinkage;
using Moq;

using Xunit;

namespace BBWM.Core.Membership.Test.ModuleLinkage;

public class RouteRolesModuleTests
{
    public RouteRolesModuleTests()
    {
    }

    private static RouteRolesModule GetService()
    {
        var mockApiAccessModelGetter = new Mock<IApiAccessModelGetter>();

        return new RouteRolesModule(mockApiAccessModelGetter.Object);
    }

    [Fact]
    public void Get_Route_Roles()
    {
        var service = GetService();

        service.GetRouteRoles();
        var result = service.GetRouteRoles();

        Assert.NotNull(result);
    }
}
