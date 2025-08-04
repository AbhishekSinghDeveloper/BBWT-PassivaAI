using BBWM.Core.Membership.Extensions;

using Xunit;

namespace BBWM.Core.Membership.Test.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void Is_User_Required_Setup_Two_Factor_Test()
    {
        var userSetuoTwoFactor = ClaimsPrincipalExtensions.IsUserRequiredSetupTwoFactor(new System.Security.Claims.ClaimsPrincipal());

        Assert.NotNull(userSetuoTwoFactor);
    }
}
