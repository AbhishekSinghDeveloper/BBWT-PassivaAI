using BBWM.Core.Membership.Utils;

using Moq;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Membership.Test.Utils;

public class RolesExtractorTests
{
    public RolesExtractorTests()
    {
    }

    [Fact]
    public void Get_All_Roles_Names_Of_Solution_Test()
    {
        var getRoleNamesOfSolution = RolesExtractor.GetAllRolesNamesOfSolution();

        Assert.NotEmpty(getRoleNamesOfSolution);
    }

    [Fact]
    public void Get_All_Roles_Names_Of_Assembly_Test()
    {
        var mockAssembly = new Mock<Assembly>();

        var getRoleNamesOfAssembly = RolesExtractor.GetAllRolesNamesOfAssembly(mockAssembly.Object);

        Assert.Empty(getRoleNamesOfAssembly);
    }

    [Fact]
    public void Get_Roles_Names_Of_Class_Test()
    {
        var mockType = new Mock<Type>();

        var getRoleNamesOfClasses = RolesExtractor.GetRolesNamesOfClass(mockType.Object);

        Assert.Empty(getRoleNamesOfClasses);
    }
}
