using BBWM.Core.Membership.Authorization;
using BBWM.Core.Membership.Model;
using BBWM.Core.Test;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Moq;

using System.Security.Claims;

using Xunit;

using static BBWM.Core.Membership.Test.AuthSecurityStampValidator.AuthSecurityStampHelpers;

using Validator = BBWM.Core.Membership.AuthTwoFactorSecurityStampValidator<BBWM.Core.Membership.Model.User>;

namespace BBWM.Core.Membership.Test.AuthSecurityStampValidator;

public class AuthTwoFactorSecurityStampValidatorTests
{
    [Fact]
    public async Task VerifySecurityStamp_Should_Validate_Two_Factor_Security_Stamp()
    {
        // Arrange
        User user = new() { UserName = "User One", AuthSecurityStamp = "ABC123" };
        IOptions<AuthSecurityStampValidatorOptions> options = CreateOptions(
            TimeSpan.FromMinutes(35), ValidationIntervalCreateMode.WithValue);
        using DataContext dataContext = await CreateDataContext(user);
        UserManager<User> userManager = ServicesFactory.GetUserManager(dataContext);
        IHttpContextAccessor httpContextAccessor =
            CreateHttpContextAccessor(user.Id, user.UserName, "ABC123");

        Mock<AuditableSignInManager> signInManager =
            ServicesFactory.GetAuditableSignInManagerMock(userManager, httpContextAccessor);
        signInManager
            .Setup(s => s.ValidateTwoFactorSecurityStampAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        CookieValidatePrincipalContext validatePrincipalContext
            = CreateCookieValidatePrincipalContext(
                httpContextAccessor.HttpContext, DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(31)));

        Validator validator =
            new(options, signInManager.Object, userManager, null, CreateLoggerFactory());

        // Act
        await validator.ValidateAsync(validatePrincipalContext);

        // Assert
        signInManager.Verify(
            s => s.ValidateTwoFactorSecurityStampAsync(It.IsAny<ClaimsPrincipal>()), Times.Once());
    }
}
