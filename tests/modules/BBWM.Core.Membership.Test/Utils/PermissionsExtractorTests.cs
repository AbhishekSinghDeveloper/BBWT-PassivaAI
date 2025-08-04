using BBWM.Core.Membership.Utils;

using Moq;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Membership.Test.Utils;

public class PermissionsExtractorTests
{
    public PermissionsExtractorTests()
    {
    }

    [Fact]
    public void Get_All_Permission_Names_Of_Solution_Test()
    {
        var getPermissionNameOfSolution = PermissionsExtractor.GetAllPermissionNamesOfSolution();

        Assert.NotNull(getPermissionNameOfSolution);
    }

    [Fact]
    public void Get_Permission_Names_Of_Class_Test()
    {
        var mockType = new Mock<Type>();

        var getPermissionNamesOfClasses = PermissionsExtractor.GetPermissionNamesOfClass(mockType.Object);

        Assert.NotNull(getPermissionNamesOfClasses);
    }

    [Fact]
    public void Get_Permission_Names_Of_Assembly_Test()
    {
        var mockAssembly = new Mock<Assembly>();

        var getPermissionNamesOfAssembly = PermissionsExtractor.GetAllPermissionNamesOfAssembly(mockAssembly.Object);

        Assert.NotNull(getPermissionNamesOfAssembly);
    }
}
