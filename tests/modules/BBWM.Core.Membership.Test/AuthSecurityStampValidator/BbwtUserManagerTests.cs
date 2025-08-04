using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;

using Microsoft.AspNetCore.Identity;

using Xunit;

namespace BBWM.Core.Membership.Test.AuthSecurityStampValidator;

public class BbwtUserManagerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("abc123")]
    public async Task CreateAsync_Should_Set_Auth_Security_Stamp(string password)
    {
        // Arrange
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        BbwtUserManager<User> bbwtUserManager = ServicesFactory.GetBbwtUserManager(dataContext);
        User user = new();

        // Act
        IdentityResult createResult = string.IsNullOrEmpty(password)
            ? await bbwtUserManager.CreateAsync(user)
            : await bbwtUserManager.CreateAsync(user, password);

        // Assert
        Assert.True(createResult.Succeeded);
        Assert.True(!string.IsNullOrEmpty(user.AuthSecurityStamp) && user.AuthSecurityStamp.Length > 0);
    }
}
