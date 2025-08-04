using BBWM.Core.Membership.Authorization;
using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

using static BBWM.Core.Membership.Test.AuthSecurityStampValidator.AuthSecurityStampHelpers;

using Validator = BBWM.Core.Membership.AuthSecurityStampValidator<BBWM.Core.Membership.Model.User>;

namespace BBWM.Core.Membership.Test.AuthSecurityStampValidator;

public class AuthSecurityStampValidatorTests
{
    [Fact]
    public async Task Ctor_Should_Throw_On_Missing_Options()
    {
        // Arrange, Act & Assert
        using DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();
        UserManager<User> userManager = ServicesFactory.GetUserManager(dataContext);
        IHttpContextAccessor httpContextAccessor = Mock.Of<IHttpContextAccessor>();
        AuditableSignInManager signInManager =
            ServicesFactory.GetAuditableSignInManager(userManager, httpContextAccessor);

        Assert.Throws<ArgumentNullException>(
            () => new Validator(null, signInManager, userManager, null, CreateLoggerFactory()));
    }

    [Theory]
    [MemberData(nameof(ShouldRejectCookieTestData))]
    public async Task ValidateAsync_Should_Reject_Cookie(
        string claimUserId,
        User user,
        string authSecurityStamp,
        ISystemClock clock,
        TimeSpan authValidationInterval,
        ValidationIntervalCreateMode intervalCreateMode,
        DateTimeOffset? issuedUtc)
    {
        // Arrange
        IOptions<AuthSecurityStampValidatorOptions> options = CreateOptions(authValidationInterval, intervalCreateMode);
        using DataContext dataContext = await CreateDataContext(user);
        UserManager<User> userManager = ServicesFactory.GetUserManager(dataContext);
        IHttpContextAccessor httpContextAccessor =
            CreateHttpContextAccessor(claimUserId ?? user?.Id, user?.UserName, authSecurityStamp);
        Mock<AuditableSignInManager> signInManager =
            ServicesFactory.GetAuditableSignInManagerMock(userManager, httpContextAccessor);
        CookieValidatePrincipalContext validatePrincipalContext
            = CreateCookieValidatePrincipalContext(httpContextAccessor.HttpContext, issuedUtc);

        Validator validator =
            new(options, signInManager.Object, userManager, clock, CreateLoggerFactory());

        // Act
        await validator.ValidateAsync(validatePrincipalContext);

        // Assert
        signInManager.Verify(s => s.SignOutAsync(), Times.Once());
    }

    [Theory]
    [MemberData(nameof(ShouldAcceptCookieTestData))]
    public async Task ValidateAsync_Should_Accept_Cookie(
        string claimUserId,
        User user,
        string authSecurityStamp,
        ISystemClock clock,
        TimeSpan authValidationInterval,
        ValidationIntervalCreateMode intervalCreateMode,
        DateTimeOffset? issuedUtc)
    {
        // Arrange
        IOptions<AuthSecurityStampValidatorOptions> options = CreateOptions(authValidationInterval, intervalCreateMode);
        using DataContext dataContext = await CreateDataContext(user);
        UserManager<User> userManager = ServicesFactory.GetUserManager(dataContext);
        IHttpContextAccessor httpContextAccessor =
            CreateHttpContextAccessor(claimUserId ?? user?.Id, user?.UserName, authSecurityStamp);
        Mock<AuditableSignInManager> signInManager =
            ServicesFactory.GetAuditableSignInManagerMock(userManager, httpContextAccessor);
        CookieValidatePrincipalContext validatePrincipalContext
            = CreateCookieValidatePrincipalContext(httpContextAccessor.HttpContext, issuedUtc);

        Validator validator =
            new(options, signInManager.Object, userManager, clock, CreateLoggerFactory());

        // Act
        await validator.ValidateAsync(validatePrincipalContext);

        // Assert
        signInManager.Verify(s => s.SignOutAsync(), Times.Never());
    }

    public static IEnumerable<object[]> ShouldRejectCookieTestData => new[]
    {
            new object[] // Interval expired (user logged out)
            {
                null,                                                               // claimUserId
                new User { UserName = "User One", AuthSecurityStamp = "ABC123" },   // user
                "ABC124",                                                           // authSecurityStamp
                CreateClock(),                                                      // clock
                default(TimeSpan),                                                  // authValidationInterval
                ValidationIntervalCreateMode.WithNullValue,                         // intervalCreateMode
                DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(30.1)),         // issuedUtc
            },
            new object[] // No Issued info should trigger validation
            {
                null,
                new User { UserName = "User One", AuthSecurityStamp = "ABC123" },
                "ABC124",
                null,
                default(TimeSpan),
                ValidationIntervalCreateMode.WithNullValue,
                null,
            },
            new object[] // User not found
            {
                "user-123",
                null,
                "ABC123",
                CreateClock(),
                TimeSpan.FromMinutes(5),
                ValidationIntervalCreateMode.WithValue,
                DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5.1)),
            },
            new object[] // AuthSecurityStamp missing from ClaimsPrincipal
            {
                null,
                new User { UserName = "User One", AuthSecurityStamp = "ABC123" },
                null,
                null,
                TimeSpan.FromMinutes(5),
                ValidationIntervalCreateMode.WithValue,
                DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5.1)),
            },
        };

    public static IEnumerable<object[]> ShouldAcceptCookieTestData => new[]
    {
            new object[] // Interval valid (invalid auth security stamp claim shouldn't get validated)
            {
                null,                                                               // claimUserId
                new User { UserName = "User One", AuthSecurityStamp = "ABC123" },   // user
                "ABC124",                                                           // authSecurityStamp
                CreateClock(),                                                      // clock
                TimeSpan.FromMinutes(5),                                            // authValidationInterval
                ValidationIntervalCreateMode.WithValue,                             // intervalCreateMode
                DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(1)),            // issuedUtc
            },
            new object[] // Interval expired (valid auth security stamp)
            {
                null,
                new User { UserName = "User One", AuthSecurityStamp = "ABC123" },
                "ABC123",
                null,
                TimeSpan.FromMinutes(5),
                ValidationIntervalCreateMode.WithValue,
                DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(6)),
            },
        };
}
