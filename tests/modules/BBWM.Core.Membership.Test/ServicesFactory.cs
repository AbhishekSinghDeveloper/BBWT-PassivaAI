using BBWM.Core.Membership.Authorization;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Membership.TokenProviders;
using BBWM.Core.Test;
using BBWM.SystemSettings;
using Bogus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BBWM.Core.Membership.Test;

public class ServicesFactory
{
    private static IUserStore<User> UserStore(DataContext dataContext)
        => new UserStore<User, Role, DataContext, string, IdentityUserClaim<string>, UserRole,
                IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>(dataContext);

    private static void SetupProviders<TUserManager>(TUserManager userManager)
        where TUserManager : UserManager<User>
    {
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
            .Setup(p => p.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserManager<User>>(),
                It.IsAny<User>()))
            .Returns(Task.FromResult(true));

        userManager.RegisterTokenProvider("Default", mock2FactorTokenProvider.Object);
        userManager.RegisterTokenProvider("Authenticator", mock2FactorTokenProvider.Object);
        userManager.RegisterTokenProvider(PasswordResetTokenProvider.ProviderName, PasswordResetTokenProvider);
    }

    public static UserManager<User> GetUserManager(DataContext context)
    {
        var userManager = new UserManager<User>(
            UserStore(context),
            Mock.Of<IOptions<IdentityOptions>>(),
            new PasswordHasher<User>(),
            new List<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>());

        SetupProviders(userManager);

        return userManager;
    }

    public static BbwtUserManager<User> GetBbwtUserManager(DataContext context)
    {
        var bbwtUserManager = new BbwtUserManager<User>(
            UserStore(context),
            Mock.Of<IOptions<IdentityOptions>>(),
            new PasswordHasher<User>(),
            new List<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>());

        SetupProviders(bbwtUserManager);

        return bbwtUserManager;
    }

    private static PasswordResetTokenProvider PasswordResetTokenProvider
    {
        get
        {
            var options = new Mock<IOptions<DataProtectionTokenProviderOptions>>();
            options.SetupGet(options => options.Value).Returns(new DataProtectionTokenProviderOptions());
            var settings = new Mock<ISettingsService>();
            settings.Setup(service => service.GetSettingsSection<UserPasswordSettings>()).Returns(new UserPasswordSettings());
            settings.Setup(service => service.GetSettingsSection<RegistrationSettings>()).Returns(new RegistrationSettings());

            return new PasswordResetTokenProvider(
                new EphemeralDataProtectionProvider(),
                options.Object,
                settings.Object,
                Mock.Of<ILogger<PasswordResetTokenProvider>>());
        }
    }

    public static RoleManager<Role> GetRoleManager(DataContext context)
    {
        var roleStore = new RoleStore<Role, DataContext, string, UserRole, IdentityRoleClaim<string>>(context);
        var roleManager = new RoleManager<Role>(
            roleStore,
            new List<IRoleValidator<Role>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<Role>>>().Object);

        return roleManager;
    }

    public static AuditableSignInManager GetAuditableSignInManager(
        UserManager<User> userManager, IHttpContextAccessor contextAccessor)
        => GetAuditableSignInManagerMock(userManager, contextAccessor).Object;

    public static Mock<AuditableSignInManager> GetAuditableSignInManagerMock(
        UserManager<User> userManager, IHttpContextAccessor contextAccessor)
    {
        var signInManager = new Mock<AuditableSignInManager>(
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IUserConfirmation<User>>().Object,
            new Mock<ILogger<AuditableSignInManager>>().Object,
            new Mock<ILoginAuditService>().Object,
            contextAccessor,
            userManager);

        signInManager.Setup(m =>
            m.SignInAsync(It.IsAny<User>(), It.IsAny<AuthenticationProperties>(), It.IsAny<string>()));

        return signInManager;
    }

    public static UserDTO GetUserEntity(string password = "password")
    {
        var faker = new Faker<UserDTO>()
            .RuleFor(p => p.Id, s => null)
            .RuleFor(p => p.FirstName, s => s.Person.FirstName)
            .RuleFor(p => p.LastName, s => s.Person.LastName)
            .RuleFor(p => p.Email, (s, p) => s.Internet.Email(p.FirstName, p.LastName))
            .RuleFor(p => p.UserName, (s, p) => p.Email)
            .RuleFor(p => p.Password, s => password)
            .RuleFor(p => p.ConfirmPassword, (s, p) => p.Password)
            .RuleFor(p => p.AccountStatus, s => AccountStatus.Unapproved)
            .RuleFor(p => p.TwoFactorEnabled, p => false)
            .RuleFor(p => p.Roles, s => new List<RoleDTO>() { })
            .RuleFor(p => p.Groups, s => new List<GroupDTO>() { });

        return faker.Generate();
    }
}
