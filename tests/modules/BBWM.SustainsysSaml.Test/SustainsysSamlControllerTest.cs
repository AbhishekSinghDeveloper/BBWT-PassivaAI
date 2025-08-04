using BBWM.Core.Membership.Model;
using BBWM.Core.Test;

using BBWT.Data;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using System.Security.Claims;

using Xunit;

namespace BBWM.SustainsysSaml.Test;

public class SustainsysSamlControllerTest
{
    public readonly IDataContext context;

    public SustainsysSamlControllerTest()
    {
        this.context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }



    [Fact]
    public async Task Index_Test()
    {
        //Arrange
        var controller = await GetController();
        var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
        urlHelperMock
            .Setup(
                x => x.Action(
                    It.IsAny<UrlActionContext>()))
            .Returns("callbackUrl")
            .Verifiable();

        controller.Url = urlHelperMock.Object;

        //Act
        var result = controller.Index();

        //Assert
        var viewResult = Assert.IsType<ChallengeResult>(result);
        var model = Assert.IsAssignableFrom<ChallengeResult>(
            viewResult);
    }

    /// <summary>
    /// method is should be based on integration test
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <param name="remoteError"></param>
    [Theory]
    [InlineData("", null)]
    [InlineData("", "test")]
    public async void ExternalLoginCallback_Test(string returnUrl = null, string remoteError = null)
    {
        //Arrange
        var controller = await GetController();

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Mock<HttpContext>().Object,
        };

        //Act
        var result = await controller.ExternalLoginCallback(returnUrl, remoteError);

        //Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        var model = Assert.IsAssignableFrom<RedirectToActionResult>(
            viewResult);
    }

    /// <summary>
    /// method is should be based on integration test
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <param name="remoteError"></param>
    //[Fact]
    public async void Login_Test()
    {
        //Arrange
        var controller = await GetController();

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult((object)null));

        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(authServiceMock.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                // How mock RequestServices?
                RequestServices = services.BuildServiceProvider()
            },
        };


        //Act
        var result = await controller.Login();

        //Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ViewResult>(
            viewResult);
    }

    [Fact]
    public async void Lockout_Test()
    {
        //Arrange
        var controller = await GetController();

        //Act
        var result = controller.Lockout();

        //Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ViewResult>(
            viewResult);
    }



    private async Task<SustainsysSamlController> GetController()
    {
        var loggerMock = new Mock<ILogger<SustainsysSamlController>>();
        var userManager = GetUserManager(context as DataContext);

        //var signInManagerMock = new SignInManager<User>(userManager, new Mock<IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<User>>().Object,
        //     new Mock<IOptions<IdentityOptions>>().Object,
        //     new Mock<ILogger<SignInManager<User>>>().Object,
        //     new Mock<IAuthenticationSchemeProvider>().Object,
        //     new Mock<IUserConfirmation<User>>().Object);

        var signInManagerMock = new Mock<SignInManager<User>>(userManager, new Mock<IHttpContextAccessor>().Object, new Mock<IUserClaimsPrincipalFactory<User>>().Object,
             new Mock<IOptions<IdentityOptions>>().Object,
             new Mock<ILogger<SignInManager<User>>>().Object,
             new Mock<IAuthenticationSchemeProvider>().Object,
             new Mock<IUserConfirmation<User>>().Object);



        return new SustainsysSamlController(signInManagerMock.Object, userManager, loggerMock.Object);
    }

    private static UserManager<User> GetUserManager(DataContext context)
    {
        //Init Store
        var store =
            new UserStore<User, Role, DataContext, string, IdentityUserClaim<string>, UserRole,
                IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>(context);

        var mockUserManager = new UserManager<User>(
            store,
            new Mock<IOptions<IdentityOptions>>().Object,
            new PasswordHasher<User>(),
            new List<IUserValidator<User>>(),
            new IPasswordValidator<User>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object);
        var mock2FactorTokenProvider = new Mock<IUserTwoFactorTokenProvider<User>>();
        mock2FactorTokenProvider
            .Setup(p => p.CanGenerateTwoFactorTokenAsync(It.IsAny<UserManager<User>>(), It.IsAny<User>()))
            .Returns(Task.FromResult(true));
        mock2FactorTokenProvider
            .Setup(p => p.GenerateAsync(It.IsAny<string>(), It.IsAny<UserManager<User>>(), It.IsAny<User>()))
            .Returns<string, UserManager<User>, User>((a, b, c) =>
            {
                c.TwoFactorEnabled = true;
                return Task.FromResult(a);
            });

        mock2FactorTokenProvider
            .Setup(p => p.ValidateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserManager<User>>(),
                It.IsAny<User>())).Returns(Task.FromResult(true));

        mockUserManager.RegisterTokenProvider("Default", mock2FactorTokenProvider.Object);
        mockUserManager.RegisterTokenProvider("Authenticator", mock2FactorTokenProvider.Object);

        return mockUserManager;
    }
}
