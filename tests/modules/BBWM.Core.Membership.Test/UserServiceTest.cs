using AutoMapper;
using BBWM.Core.AppEnvironment;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Extensions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.ModelHashing;
using BBWM.Core.Test;
using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using BBWM.Core.Utils;
using BBWM.Messages;
using BBWM.Messages.Templates;
using BBWM.ReCaptcha;
using BBWM.SystemSettings;
using BBWT.Data;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using BBWM.Core.Extensions;
using U2F.Core.Exceptions;
using U2F.Core.Utils;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class UserServiceTest : IClassFixture<MappingFixture>
{

    private const string SampleUserPassword = "password";
    public IMapper Mapper { get; }

    public UserServiceTest(MappingFixture mappingFixture)
        => Mapper = mappingFixture.DefaultMapper;

    /// <inheritdoc />
    /// <summary>
    /// Uncomment this constructor to instantiate the Test with an output helper to get debug info in test results
    /// </summary>
    /// public UserServiceTest(ITestOutputHelper output) :base(output) { }
    /// <summary>
    /// An override of a template method to get the instance of CRUD Service being tested.
    /// Context depends on internal TestBase implementation - i.e. could be same during several calls (for persistance between transactions testing),
    /// or could be new InMemory DB per call (clear test)
    /// </summary>
    /// <param name="context">InMemory DB Context</param>
    /// <returns></returns>
    private IUserService GetService(IDataContext context, UserManager<User> userManager = default)
    {
        // trick to attach to process to debug this particular test via VS Code
        // while(!Debugger.IsAttached) Thread.Sleep(500);

        if (context is not DataContext ctx)
        {
            throw new InvalidCastException();
        }

        userManager ??= ServicesFactory.GetUserManager(ctx);

        var roleManager = ServicesFactory.GetRoleManager(ctx);
        var signInManager = ServicesFactory.GetAuditableSignInManager(
            userManager, Core.Test.ServicesFactory.GetHttpContextAccessor());

        var settingService = new SettingsService(ctx, new SettingsSectionService(), null);

        // modelHashingService registered as a singleton
        var modelHashingService = new ModelHashingService();
        modelHashingService.Register(Mapper, ctx);

        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(p => p.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile[]>(), It.IsAny<EmailBrandInfo>(), It.IsAny<string[]>())).Returns(Task.FromResult<object>(null));

        var auditService = new Mock<ILoginAuditService>();
        auditService.Setup(a => a.GetLastAttemptsCountAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(1);

        var dataService = SutDataHelper.CreateEmptyDataService(Mapper, ctx: (IDbContext)ctx);

        var httpContextAccessor = Core.Test.ServicesFactory.GetHttpContextAccessor();

        var securityService = new SecurityService(ctx, httpContextAccessor, userManager, settingService, auditService.Object);
        var allowedIpService = new AllowedIpService(ctx, userManager);

        var emailTemplateService = new EmailTemplateService(ctx, dataService, settingService);
        var reCaptchaAppSettingsOptions = new Mock<IOptionsSnapshot<ReCaptchaAppSettings>>().Object;
        var httpClientFactory = new Mock<IHttpClientFactory>().Object;
        var reCaptchaService = new ReCaptchaService(reCaptchaAppSettingsOptions, settingService, httpClientFactory, new Mock<ILogger<ReCaptchaService>>().Object);
        var loginAuditService = new LoginAuditService(httpContextAccessor, new Mock<ILogger<LoginAuditService>>().Object, ctx);

        var templatesDataLinkage = new BBWM.Messages.Templates.DataModuleLinkage();
        templatesDataLinkage.EnsureInitialData(ctx).Wait();

        var mockPwnedPasswordProvider = new Mock<IPwnedPasswordProvider>();
        mockPwnedPasswordProvider.Setup(a => a.GetPasswordPwned(It.IsAny<string>())).ReturnsAsync("37D0679CA88DB6464EAC60DA96345513964:2389787");

        var loginSetting = new Mock<IOptionsSnapshot<UserLoginSettings>>();
        loginSetting.SetupGet(opt => opt.Value).Returns(new UserLoginSettings());

        var appEnvironmentService = new AppEnvironmentService(
            Core.Test.ServicesFactory.GetWebHostEnvironment(false),
            null);

        var userDataService = new UserDataService(
            dataService,
            modelHashingService,
            userManager,
            roleManager,
            context,
            Mapper);

        var user2FAService = new User2FAService(userManager, UrlEncoder.Default, loginSetting.Object,
            loginAuditService, settingService);

        return new UserService(
            dataService,
            userDataService,
            userManager,
            signInManager,
            loginSetting.Object,
            securityService,
            allowedIpService,
            emailSender.Object,
            emailTemplateService,
            httpContextAccessor,
            mockPwnedPasswordProvider.Object,
            settingService,
            reCaptchaService,
            loginAuditService,
            ctx,
            appEnvironmentService,
            user2FAService,
            Mapper);
    }

    private IUserDataService CreateUserDataService(IDataContext context)
    {
        if (context is not DataContext ctx)
        {
            throw new InvalidCastException();
        }

        var dataService = SutDataHelper.CreateEmptyDataService(Mapper, ctx: (IDbContext)ctx);

        var modelHashingService = new ModelHashingService();
        modelHashingService.Register(Mapper, ctx);

        var userManager = ServicesFactory.GetUserManager(ctx);

        var roleManager = ServicesFactory.GetRoleManager(ctx);

        return new UserDataService(
            dataService,
            modelHashingService,
            userManager,
            roleManager,
            ctx,
            Mapper);
    }

    private static RoleDTO GetFakerRole()
    {
        var faker = new Faker<RoleDTO>()
            .RuleFor(p => p.Id, s => s.Random.Guid().ToString())
            .RuleFor(p => p.Name, s => s.Random.AlphaNumeric(10))
            .RuleFor(p => p.AuthenticatorRequired, s => false)
            .RuleFor(p => p.CheckIp, s => false);

        return faker.Generate();
    }

    private static LoginDTO FakeLoginDTO()
    {
        var loginDto = new Faker<LoginDTO>()
               .RuleFor(p => p.Email, s => "test@mail.com")
               .RuleFor(p => p.Password, s => SampleUserPassword)
               .RuleFor(p => p.CaptchaResponse, s => s.Random.AlphaNumeric(7))
               .RuleFor(p => p.Fingerprint, s => s.Random.AlphaNumeric(7))
               .RuleFor(p => p.Browser, s => s.Random.AlphaNumeric(7))
               .RuleFor(p => p.TwoFactorCode, s => s.Random.AlphaNumeric(7))
               .RuleFor(p => p.TwoFactorRecoveryCode, s => s.Random.AlphaNumeric(7))
               .RuleFor(p => p.RealFirstName, s => s.Random.AlphaNumeric(7))
               .RuleFor(p => p.RealLastName, s => s.Random.AlphaNumeric(7))
               .RuleFor(p => p.RealEmail, s => "test@mail.com");

        return loginDto.Generate();
    }

    [Fact]
    public async Task Save_New_User()
    {
        // Arrange
        var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var userDto = ServicesFactory.GetUserEntity();

        // Act
        var userId = (await userDataService.Create(userDto, CancellationToken.None)).Id;
        userDto.Id = userId;

        var email = userDto.Email;
        var user = await ctx.Set<User>().FirstOrDefaultAsync(u => u.Email == email);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(user.Id, userDto.Id);
        Assert.Equal(user.Email, userDto.Email);
    }

    [Fact]
    public async Task Is_User_Impersonating_Test()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var user = ServicesFactory.GetUserEntity();
        var sut = GetService(ctx);

        var mock = new ClaimsPrincipal();
        var test = await sut.IsUserImpersonating(mock);

        Assert.NotNull(mock);
        Assert.NotNull(test);
        Assert.False(test.IsImpersonating);
    }

    [Fact]
    public async Task Get_All_Groups()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var user = ServicesFactory.GetUserEntity();

        await userDataService.Create(user, CancellationToken.None);
        var test = userDataService.GetAllGroups();

        Assert.NotNull(test);
    }

    [Fact]
    public async Task Get_All_Roles()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var user = ServicesFactory.GetUserEntity();

        await userDataService.Create(user, CancellationToken.None);
        var test = await userDataService.GetAllRoles();
        var test2 = userDataService.GetAllAccountStatuses();

        Assert.NotNull(test);
        Assert.NotNull(test2);
    }

    [Fact]
    public async Task Resend_Invitations_Test()
    {
        // Arrange
        const string USER_ID = "test@email.com";

        var userDto = ServicesFactory.GetUserEntity();
        userDto.Id = USER_ID;

        var dataService =
            await SutDataHelper.CreateDataServiceWithData<IDataContext, User, UserDTO>(Mapper, new[] { userDto });
        using var ctx = dataService.Context;

        var newUserId = ctx.Set<User>().SingleOrDefault(u => u.Id == USER_ID);

        var fakeToken = new Faker<ActivationToken>()
            .RuleFor(p => p.Id, s => s.Random.Int())
            .RuleFor(p => p.Token, s => s.Random.AlphaNumeric(7))
            .RuleFor(p => p.ExpirationDate, s => new DateTime())
            .Generate();

        newUserId.InvitationToken = null;
        newUserId.AccountStatus = AccountStatus.Invited;

        // Act
        var sut = GetService(ctx);

        // Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.ResendInvitation(null, CancellationToken.None));

        await Assert.ThrowsAsync<ConflictException>(() => sut.ResendInvitation(newUserId.Id, CancellationToken.None));

        newUserId.AccountStatus = AccountStatus.Active;
        await ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessException>(() => sut.ResendInvitation(newUserId.Id, CancellationToken.None));

        newUserId.AccountStatus = AccountStatus.Invited;
        newUserId.InvitationToken = fakeToken;
        await sut.ResendInvitation(newUserId.Id, CancellationToken.None);
    }

    [Fact]
    public async Task Save_Existent_User()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var userService = GetService(ctx);

        var userDto = ServicesFactory.GetUserEntity();

        // Act
        userDto.Id = (await userDataService.Create(userDto)).Id;
        userDto.Email = "newemail@gmail.com";
        userDto.AccountStatus = AccountStatus.Active;

        var newUserID = (await userDataService.Update(userDto)).Id;

        var user = ctx.Set<User>().SingleOrDefault(u => u.Email == userDto.Email);

        // Assert
        Assert.False(user.EmailConfirmed);
        Assert.Equal(AccountStatus.Unapproved, user.AccountStatus);

        await userService.Approve(user.Id, CancellationToken.None);
        Assert.Equal(AccountStatus.Unverified, user.AccountStatus);
    }

    [Fact]
    public async Task Save_Existent_User_And_Resend_Notifications_After_Email_Changing()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var userDto = ServicesFactory.GetUserEntity();

        // Act
        userDto.Id = (await userDataService.Create(userDto)).Id;
        userDto.Email = "newemail@gmail.com";

        var newUserID = (await userDataService.Update(userDto)).Id;

        var user = ctx.Set<User>().SingleOrDefault(u => u.Email == userDto.Email);

        // Assert
        Assert.Equal(newUserID, userDto.Id);
        Assert.Equal(user.Email, userDto.Email);
    }

    [Fact]
    public async Task Invited_User_Must_Have_InvitationToken()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDto = ServicesFactory.GetUserEntity();
        var sut = GetService(ctx);

        // Act
        var invitedUserDTO = await sut.Invite(userDto, CancellationToken.None);
        var invitedUser = ctx.Set<User>().FirstOrDefault(u => u.Id == invitedUserDTO.Id);

        // Assert
        Assert.NotNull(invitedUser);
        Assert.Equal(AccountStatus.Invited, invitedUser.AccountStatus);
        Assert.NotNull(invitedUser.InvitationTokenId);
    }

    // [Fact]
    public async Task Must_Replace_Users_Roles()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = CreateUserDataService(ctx);

        var userDto = ServicesFactory.GetUserEntity();
        var initialUser = Mapper.Map<User>(userDto);

        var fakerRoleForAdd1 = Mapper.Map<Role>(GetFakerRole());
        var fakerRoleForAdd2 = Mapper.Map<Role>(GetFakerRole());
        var fakerRoleForAdd3 = Mapper.Map<Role>(GetFakerRole());
        var fakerRoleForRemove = Mapper.Map<Role>(GetFakerRole());

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);
        var roleManager = ServicesFactory.GetRoleManager(ctx as DataContext);

        await userManager.CreateAsync(initialUser);

        await roleManager.CreateAsync(fakerRoleForAdd1);
        await roleManager.CreateAsync(fakerRoleForAdd2);
        await roleManager.CreateAsync(fakerRoleForAdd3);
        await roleManager.CreateAsync(fakerRoleForRemove);

        var role = await roleManager.FindByNameAsync(fakerRoleForAdd1.Name);

        await userManager.AddToRoleAsync(initialUser, fakerRoleForAdd1.Name);
        await userManager.AddToRoleAsync(initialUser, fakerRoleForRemove.Name);

        var userRoleReplDTO = new UsersRolesReplacementDTO()
        {
            UsersIds = new List<string>() { initialUser.Id },
            RolesIdsToAdd = new List<string>() { fakerRoleForAdd2.Id, fakerRoleForAdd3.Id },
            RolesIdsToRemove = new List<string>() { fakerRoleForRemove.Id },
        };

        // Act
        var invitedUserDTO = await sut.ReplaceUsersRoles(userRoleReplDTO, CancellationToken.None);

        // Assert
        Assert.NotEmpty(invitedUserDTO);
        Assert.Equal(3, invitedUserDTO.FirstOrDefault().Roles.Count());
        Assert.Contains(invitedUserDTO.FirstOrDefault().Roles, r => r.Id == fakerRoleForAdd2.Id);
        Assert.Contains(invitedUserDTO.FirstOrDefault().Roles, r => r.Id == fakerRoleForAdd3.Id);

        Assert.DoesNotContain(invitedUserDTO.FirstOrDefault().Roles, r => r.Id == fakerRoleForRemove.Id);
    }

    [Fact]
    public async Task User_Must_Be_Registered()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var userRegistrationDTO = new UserRegistrationDTO()
        {
            Email = "testemail@gmail.com",
            FirstName = "FirstName",
            LastName = "LastName",
            Password = "pass",
            PasswordSHA1 = "passSha",
        };
        var userDto = ServicesFactory.GetUserEntity();

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.Email = "testemail@gmail.com";

        // Act
        await sut.Register(userRegistrationDTO, CancellationToken.None);
        var user = ctx.Set<User>().FirstOrDefault(u => u.Email == "testemail@gmail.com");
        var pasHist = ctx.Set<PasswordHistory>().FirstOrDefault(p => p.UserId == user.Id);
        ctx.SaveChanges();

        // Assert
        Assert.NotNull(userMapper);
        Assert.NotNull(user);
        Assert.NotNull(pasHist);
        Assert.Equal(AccountStatus.Unapproved, user.AccountStatus);
    }

    [Fact]
    public async Task Password_Is_Pwned()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);

        var userRegistrationDTO = new UserRegistrationDTO()
        {
            Email = "testemail@gmail.com",
            FirstName = "FirstName",
            LastName = "LastName",
            Password = "8cb2237d0679ca88db6464eac60da96345513964",
            PasswordSHA1 = "8cb2237d0679ca88db6464eac60da96345513964",
        };

        // Act
        var nbr = await sut.CheckPwnedPassword(userRegistrationDTO);

        // Assert
        Assert.NotEqual(0, nbr);
    }

    [Fact]
    public async Task Get_Token_After_Resend_Email_Confir()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var sut = GetService(ctx);

        var userDto = ServicesFactory.GetUserEntity();
        userDto.AccountStatus = AccountStatus.Active;

        // Act
        userDto.Id = (await userDataService.Create(userDto)).Id;

        var userBeforeHasEmailConfirmaitonToken = ctx.Set<User>().SingleOrDefault(u => u.Email == userDto.Email).EmailConfirmationTokenId.HasValue;

        Func<Task> result = async () => await sut.ResendEmailConfirmation(userDto.Id, CancellationToken.None);

        var user = ctx.Set<User>().SingleOrDefault(u => u.Email == userDto.Email);

        Func<Task> result2 = async () => await sut.ResendEmailConfirmation(userDto.Id, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<BusinessException>(result);
        Assert.False(userBeforeHasEmailConfirmaitonToken);
        Assert.False(user.EmailConfirmationTokenId.HasValue);
    }

    [Fact]
    public async Task Refresh_Two_Factor_Setup_Claims_Test()
    {
        var userDto = ServicesFactory.GetUserEntity();

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.UserRoles.Add(new UserRole
        {
            User = userMapper,
            Role = new Role { Name = Core.Roles.SystemAdminRole },
        });
        userMapper.AuthenticationRequests = new List<AuthenticationRequest>()
            {
                new AuthenticationRequest() { Id = 1, AppId = "1", Challenge = "test", KeyHandle = "testkeyhandle", Version = "1" },
                new AuthenticationRequest() { Id = 2, AppId = "2", Challenge = "test2", KeyHandle = "testkey2handle", Version = "2" },
            };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { userMapper });
        var service = GetService(ctx);

        await service.RefreshTwoFactorSetupClaims(userMapper, CancellationToken.None);
    }

    [Fact]
    public async Task Reset_Password_With_Verify_Must_Remove_Password_Token()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        var tokenCode = Guid.NewGuid().ToString();
        user.Email = "test@gmail.com";
        user.PasswordResetToken = new ActivationToken { Token = tokenCode };
        user.AccountStatus = AccountStatus.Active;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        await userManager.CreateAsync(user);

        // Act
        var resetPassDTO = new ResetPasswordDTO() { Email = "test@gmail.com", Password = "passNew", Code = tokenCode };
        await sut.ResetPassword(resetPassDTO, CancellationToken.None);

        user.AccountStatus = AccountStatus.Unverified;

        Func<Task> result = () => sut.ResetPassword(resetPassDTO, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<BusinessException>(result);
        Assert.False(user.PasswordResetTokenId.HasValue);
    }

    [Fact]
    public async Task Reset_Password_Test_Returns_Business_Exceptions()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        var tokenCode = Guid.NewGuid().ToString();
        user.Email = "test@gmail.com";
        user.PasswordResetToken = new ActivationToken { Token = tokenCode };
        user.AccountStatus = AccountStatus.Unverified;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        await userManager.CreateAsync(user);

        // Act
        var resetPassDTO = new ResetPasswordDTO() { Email = "test@gmail.com", Password = "passNew", Code = tokenCode };

        Func<Task> result = () => sut.ResetPassword(resetPassDTO, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<BusinessException>(result);
    }

    [Fact]
    public async Task Reset_Password_Must_Unlock_User()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var sut = GetService(ctx);

        var serviceCollection = new ServiceCollection();
        var identityBuilder = new IdentityBuilder(typeof(User), serviceCollection);
        identityBuilder.AddDefaultTokenProviders();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var userDto = ServicesFactory.GetUserEntity();
        userDto.AccountStatus = AccountStatus.Unapproved;

        await userDataService.Create(userDto, CancellationToken.None);
        var resetPasswordDto = new ResetPasswordDTO()
        {
            Email = userDto.Email,
            Password = "testchangepasword",
        };
        var emptyDto = new ResetPasswordDTO() { Email = "", Password = "testchangepassword" };

        // Act
        await sut.ResetPassword(resetPasswordDto, CancellationToken.None, false);

        Func<Task> result = () => sut.ResetPassword(emptyDto, CancellationToken.None);
        var user = ctx.Set<User>().SingleOrDefault(u => u.Email == userDto.Email);

        // Assert
        await Assert.ThrowsAsync<ObjectNotExistsException>(result);
        Assert.False(user.LockoutEnabled);
        Assert.Null(user.FirstPasswordFailureDate);
    }

    [Fact]
    public async Task Confirm_Email_Must_Activate_User()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var user2 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var user3 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var user4 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var user5 = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        var tokenCode2 = Guid.NewGuid().ToString();
        user2.AccountStatus = AccountStatus.Suspended;
        user2.EmailConfirmationToken = new ActivationToken() { Token = tokenCode2 };

        var tokenCode3 = Guid.NewGuid().ToString();
        user3.AccountStatus = AccountStatus.Deleted;
        user3.EmailConfirmationToken = new ActivationToken() { Token = tokenCode3 };

        var tokenCode4 = Guid.NewGuid().ToString();
        user4.AccountStatus = AccountStatus.Active;
        user4.EmailConfirmationToken = new ActivationToken() { Token = tokenCode4 };

        var tokenCode5 = Guid.NewGuid().ToString();
        user4.AccountStatus = AccountStatus.Active;

        var tokenCode6 = Guid.NewGuid().ToString();
        user5.EmailConfirmationToken = null;
        user.AccountStatus = AccountStatus.Unverified;

        var now = DateTime.Now;
        var expires = now.AddDays(-10);

        user4.EmailConfirmationToken.ExpirationDate = expires;

        var sut = GetService(ctx);
        var tokenCode = Guid.NewGuid().ToString();
        user.AccountStatus = AccountStatus.Unverified;
        user.EmailConfirmationToken = new ActivationToken() { Token = tokenCode };

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);
        await userManager.CreateAsync(user);
        await userManager.CreateAsync(user2);
        await userManager.CreateAsync(user3);
        await userManager.CreateAsync(user4);
        await userManager.CreateAsync(user5);

        // Act
        var confirmEmailDTO = new ConfirmEmailDTO() { UserId = user.Id, Code = tokenCode };
        var confirmEmailDTO2 = new ConfirmEmailDTO() { UserId = user2.Id, Code = tokenCode2 };
        var confirmEmailDTO3 = new ConfirmEmailDTO() { UserId = user3.Id, Code = tokenCode3 };
        var confirmEmailDTO4 = new ConfirmEmailDTO() { UserId = user4.Id, Code = tokenCode4 };
        var confirmEmailDTO5 = new ConfirmEmailDTO() { UserId = user4.Id, Code = tokenCode5 };
        var confirmEmailDTO6 = new ConfirmEmailDTO() { UserId = user5.Id, Code = tokenCode4 };
        var emptyEmailDto = new ConfirmEmailDTO() { UserId = user.Id, Code = "" };
        await sut.ConfirmEmail(confirmEmailDTO, CancellationToken.None);

        Func<Task> result = () => sut.ConfirmEmail(emptyEmailDto, CancellationToken.None);

        Func<Task> result2 = () => sut.ConfirmEmail(confirmEmailDTO2, CancellationToken.None);

        Func<Task> result3 = () => sut.ConfirmEmail(confirmEmailDTO3, CancellationToken.None);

        Func<Task> result4 = () => sut.ConfirmEmail(confirmEmailDTO4, CancellationToken.None);

        Func<Task> result5 = () => sut.ConfirmEmail(confirmEmailDTO5, CancellationToken.None);

        Func<Task> result6 = () => sut.ConfirmEmail(confirmEmailDTO6, CancellationToken.None);

        // Assert
        Assert.NotNull(emptyEmailDto);
        await Assert.ThrowsAsync<BusinessException>(result);
        await Assert.ThrowsAsync<BusinessException>(result2);
        await Assert.ThrowsAsync<BusinessException>(result3);
        await Assert.ThrowsAsync<BusinessException>(result4);
        await Assert.ThrowsAsync<BusinessException>(result5);
        await Assert.ThrowsAsync<BusinessException>(result6);
        Assert.Equal(AccountStatus.Active, user.AccountStatus);
        Assert.Equal(AccountStatus.Suspended, user2.AccountStatus);
        Assert.Equal(AccountStatus.Deleted, user3.AccountStatus);
        Assert.Equal(AccountStatus.Active, user4.AccountStatus);
    }

    [Fact]
    public async Task Update_User_Permissions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var userDto = ServicesFactory.GetUserEntity();

        int userId = 1, permId = 1;
        var userPermission = new Faker<UserPermission>()
            .RuleFor(p => p.UserId, s => userId++.ToString())
            .RuleFor(p => p.User, s => new User())
            .RuleFor(p => p.PermissionId, s => permId)
            .RuleFor(p => p.Permission, s => new Permission() { Name = $"Perm{permId++}" })
            .Generate(3);

        await userDataService.Create(userDto);

        // Act
        var userMapper = Mapper.Map<User>(userDto);
        userMapper.UserPermissions = userPermission;
        await ctx.Set<User>().AddAsync(userMapper);
        await ctx.SaveChangesAsync();

        // Assert
        Assert.NotNull(userMapper);
        Assert.NotNull(userPermission.Find(x => x.UserId == userMapper.Id));
    }

    //[Fact]
    //public async Task Approve_Must_Unverify_User()
    //{
    //    // Arrange
    //    using var ctx = datatFixture.CreateContext<IDataContext>();
    //    var userDto = GetEntity();
    //    var userDto2 = GetEntity();
    //    var sut = CreateUserDataService(ctx);

    //    Action deleteAllException = () => sut.DeleteAll(CancellationToken.None);

    //    sut.GetPage(new QueryCommand(), CancellationToken.None);
    //    sut.GetPage(CancellationToken.None);
    //    sut.GetPage<UserDTO>();

    //    Expression<Func<UserDTO, bool>> myExpression = b => b.IsSystemTester;

    //    sut.Get(myExpression, CancellationToken.None);

    //    sut.Get<UserDTO>(It.IsAny<string>());
    //    sut.GetAll(CancellationToken.None);
    //    sut.GetAll(new Filter(), CancellationToken.None);

    //    sut.Exists(myExpression, CancellationToken.None);

    //    userDto.AccountStatus = AccountStatus.Unapproved;
    //    userDto2.AccountStatus = AccountStatus.Active;

    //    userDto.Id = GetId(await sut.Save(userDto, CancellationToken.None));
    //    userDto2.Id = GetId(await sut.Save(userDto2, CancellationToken.None));

    //    // Act
    //    await sut.Approve(userDto.Id, CancellationToken.None);
    //    Func<Task> result = async () => await sut.Approve(userDto2.Id, CancellationToken.None);

    //    var user = ctx.Set<User>().SingleOrDefault(u => u.Email == userDto.Email);
    //    var user2 = ctx.Set<User>().SingleOrDefault(u => u.Email == userDto2.Email);
    //    // Assert
    //    Assert.NotNull(user);
    //    Assert.NotNull(user2);
    //    Assert.NotNull(sut);

    //    Assert.Throws<NotImplementedException>(deleteAllException);

    //    Assert.Equal(AccountStatus.Unverified, user.AccountStatus);
    //    Assert.True(user.EmailConfirmationTokenId.HasValue);
    //    await Assert.ThrowsAsync<BusinessException>(result);
    //}

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Toogle_Loocking_Must_Toogle_LockoutEnabled(bool lockoutEnabled)
    {
        // Arrange
        var activeUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        activeUser.AccountStatus = AccountStatus.Active;
        activeUser.LockoutEnabled = lockoutEnabled;
        activeUser.PreviousAccountStatus = AccountStatus.Unapproved;

        var suspendedUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        suspendedUser.AccountStatus = AccountStatus.Suspended;
        suspendedUser.LockoutEnabled = lockoutEnabled;
        suspendedUser.PreviousAccountStatus = AccountStatus.Active;

        var unapprovedUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        unapprovedUser.AccountStatus = AccountStatus.Unapproved;
        unapprovedUser.LockoutEnabled = lockoutEnabled;
        unapprovedUser.PreviousAccountStatus = null;

        var suspendedUser2 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        suspendedUser2.AccountStatus = AccountStatus.Suspended;
        suspendedUser2.LockoutEnabled = lockoutEnabled;
        suspendedUser2.PreviousAccountStatus = null;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(
            new[] { activeUser, suspendedUser, unapprovedUser, suspendedUser2 });
        var sut = GetService(ctx);

        // Act
        await sut.ToggleLocking(activeUser.Id, CancellationToken.None);
        await sut.ToggleLocking(suspendedUser.Id, CancellationToken.None);
        Func<Task> result = () => sut.ToggleLocking(unapprovedUser.Id, CancellationToken.None);
        Func<Task> result2 = () => sut.ToggleLocking(suspendedUser2.Id, CancellationToken.None);

        var activeUserAfter = ctx.Set<User>().Single(u => u.Email == activeUser.Email);
        var suspendedUserAfter = ctx.Set<User>().Single(u => u.Email == suspendedUser.Email);
        var unapprovedUserAfter = ctx.Set<User>().Single(u => u.Email == unapprovedUser.Email);
        var activeUser2After = ctx.Set<User>().Single(u => u.Email == suspendedUser2.Email);

        // Assert
        Assert.True(activeUserAfter.LockoutEnabled);
        Assert.Equal(AccountStatus.Suspended, activeUserAfter.AccountStatus);
        Assert.False(suspendedUserAfter.LockoutEnabled);
        Assert.Equal(AccountStatus.Active, suspendedUserAfter.AccountStatus);
        await Assert.ThrowsAsync<BusinessException>(result);
        await Assert.ThrowsAsync<ConflictException>(result2);
    }

    [Fact]
    public async Task User_Must_Not_Be_Deleted_By_ToggleDeleting()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var user2 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.AccountStatus = AccountStatus.Deleted;
        user.PreviousAccountStatus = AccountStatus.Unapproved;

        user2.AccountStatus = AccountStatus.Deleted;
        user2.PreviousAccountStatus = null;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user, user2 });
        var sut = GetService(ctx);

        // Act
        await sut.ToggleDeleting(user.Id, CancellationToken.None);
        Func<Task> result = () => sut.ToggleDeleting(user2.Id, CancellationToken.None);

        var userAfter = ctx.Set<User>().SingleOrDefault(u => u.Email == user.Email);
        var user2After = ctx.Set<User>().SingleOrDefault(u => u.Email == user2.Email);

        // Assert
        Assert.Equal(AccountStatus.Unapproved, userAfter.AccountStatus);
        Assert.False(userAfter.LockoutEnabled);
        Assert.Null(userAfter.PreviousAccountStatus);
        await Assert.ThrowsAsync<ConflictException>(result);
    }

    [Fact]
    public async Task User_Must_Be_Deleted_By_ToggleDeleting()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.AccountStatus = AccountStatus.Active;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        await sut.ToggleDeleting(user.Id, CancellationToken.None);

        var userAfter = ctx.Set<User>().SingleOrDefault(u => u.Email == user.Email);

        // Assert
        Assert.Equal(AccountStatus.Deleted, userAfter.AccountStatus);
        Assert.Equal(AccountStatus.Active, userAfter.PreviousAccountStatus);
    }

    [Fact]
    public async Task ExistsTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = CreateUserDataService(ctx);

        // Act
        var shouldExistResult = await sut.Exists(user.Id);
        var shouldntExistResult = await sut.Exists(user.Id + "test");

        // Assert
        Assert.True(shouldExistResult);
        Assert.False(shouldntExistResult);
    }

    [Fact]
    public async Task GetByEmailTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = CreateUserDataService(ctx);

        // Act
        var result = await sut.GetByEmail(user.Email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FirstName, result.FirstName);
    }

    [Fact]
    public async Task CheckRecoveryCodeExistsTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.PasswordResetToken = new ActivationToken { Token = "test", ExpirationDate = DateTime.Today.AddDays(1) };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var dto = new RecoveryCodeDTO { UserId = user.Id, Code = "test" };

        // Act
        var exception = await Record.ExceptionAsync(() => sut.CheckRecoveryCodeExists(dto));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task CheckRecoveryCodeExists_Exceptions_Test()
    {
        // Arrange
        var userWithoutResetToken = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var userWithExpiredToken = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        userWithExpiredToken.PasswordResetToken = new ActivationToken { Token = "expired", ExpirationDate = DateTime.Today.AddDays(-1) };
        var userWithCorrectData = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        userWithCorrectData.PasswordResetToken = new ActivationToken { Token = "correct", ExpirationDate = DateTime.Today.AddDays(1) };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(
            new[] { userWithoutResetToken, userWithExpiredToken, userWithCorrectData });
        var sut = GetService(ctx);

        var wrongToken = new ActivationToken { Token = "wrong", ExpirationDate = DateTime.Today.AddDays(1) };
        ctx.Set<ActivationToken>().Add(wrongToken);
        await ctx.SaveChangesAsync();

        var recoveryDTONotExistingToken = new RecoveryCodeDTO { Code = "not exist", UserId = userWithoutResetToken.Id };
        var recoveryDTONotExistingCode = new RecoveryCodeDTO { Code = "not exist", UserId = userWithCorrectData.Id };
        var recoveryDTOWrongCode = new RecoveryCodeDTO { Code = "wrong", UserId = userWithCorrectData.Id };
        var recoveryDTOExpiredDate = new RecoveryCodeDTO { Code = "expired", UserId = userWithExpiredToken.Id };

        // Act & Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.CheckRecoveryCodeExists(new RecoveryCodeDTO()));
        var notExisstingTokenException = await Assert.ThrowsAsync<BusinessException>(() => sut.CheckRecoveryCodeExists(recoveryDTONotExistingToken));
        await Assert.ThrowsAsync<ObjectNotExistsException>(() => sut.CheckRecoveryCodeExists(recoveryDTONotExistingCode));
        var expiredDateException = await Assert.ThrowsAsync<BusinessException>(() => sut.CheckRecoveryCodeExists(recoveryDTOExpiredDate));
        var wrongCodeException = await Assert.ThrowsAsync<BusinessException>(() => sut.CheckRecoveryCodeExists(recoveryDTOWrongCode));
        Assert.Equal(UserService.ErrorMessages.RecoveryNotFoundForUser, notExisstingTokenException.Message);
        Assert.Equal(UserService.ErrorMessages.RecoveryExpired, expiredDateException.Message);
        Assert.Equal(UserService.ErrorMessages.RecoveryInvalid, wrongCodeException.Message);
    }

    [Fact]
    public async Task GetAccountActivationInfoTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.AccountStatus = AccountStatus.Invited;
        user.InvitationToken = new ActivationToken
        {
            Token = "test",
            ExpirationDate = new DateTime(),
        };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        var result = await sut.GetAccountActivationInfo(user.Id, user.InvitationToken.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.True(result.IsInvited);
    }

    [Fact]
    public async Task GetAccountActivationInfo_Exceptions_Test()
    {
        // Arrange
        var userActive = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        userActive.AccountStatus = AccountStatus.Active;
        var userWithoutInvitation = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        userWithoutInvitation.AccountStatus = AccountStatus.Invited;
        var userWithWrongCode = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        userWithWrongCode.AccountStatus = AccountStatus.Invited;
        userWithWrongCode.InvitationToken = new ActivationToken
        {
            Token = "wrong",
            ExpirationDate = new DateTime(),
        };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(
            new[] { userActive, userWithoutInvitation, userWithWrongCode });
        var sut = GetService(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.GetAccountActivationInfo(string.Empty, string.Empty));
        var alreadyActivatedException = await Assert.ThrowsAsync<BusinessException>(() => sut.GetAccountActivationInfo(userActive.Id, string.Empty));
        var notFoundInvitationException = await Assert.ThrowsAsync<BusinessException>(() => sut.GetAccountActivationInfo(userWithoutInvitation.Id, string.Empty));
        var wrongActivationCodeException = await Assert.ThrowsAsync<BusinessException>(() => sut.GetAccountActivationInfo(userWithWrongCode.Id, "right"));
        Assert.Equal(ActivationError.ActivationCompleted.ToEnumValueString(), alreadyActivatedException.Message);
        Assert.Equal(ActivationError.InvitationNotFoundForUser.ToEnumValueString(), notFoundInvitationException.Message);
        Assert.Equal(ActivationError.ActivationCodeInvalid.ToEnumValueString(), wrongActivationCodeException.Message);
    }

    [Fact]
    public async Task CanImpersonateTest()
    {
        // Arrange
        var impresonatingUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        impresonatingUser.UserRoles.Add(new UserRole
        {
            User = impresonatingUser,
            Role = new Role { Name = Core.Roles.SystemAdminRole },
        });
        var impresonatedUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var cantImprisonateUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(
            new[] { impresonatingUser, impresonatedUser, cantImprisonateUser });
        var sut = GetService(ctx);

        // Act
        var shouldImpresonate = await sut.CanImpersonate(impresonatingUser.Id, impresonatedUser.Id);
        var shouldntImpresonate = await sut.CanImpersonate(cantImprisonateUser.Id, impresonatedUser.Id);

        // Assert
        Assert.True(shouldImpresonate);
        Assert.False(shouldntImpresonate);
    }

    [Fact]
    public async Task CanImpersonateTest_Exceptions_Test()
    {
        // Arrange
        var impresonatingUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { impresonatingUser });
        var sut = GetService(ctx);

        // Act
        var impresonatingUserException = await Assert.ThrowsAsync<UserNotExistsException>(() => sut.CanImpersonate(string.Empty, string.Empty));
        var impresonatedUserException = await Assert.ThrowsAsync<UserNotExistsException>(() => sut.CanImpersonate(impresonatingUser.Id, string.Empty));

        // Assert
        Assert.Equal(UserService.ErrorMessages.ImpersonatingUserNotFound, impresonatingUserException.Message);
        Assert.Equal(UserService.ErrorMessages.ImpersonatedUserNotFound, impresonatedUserException.Message);
    }

    [Fact]
    public async Task InviteTest()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var userDTO = ServicesFactory.GetUserEntity();

        // Act
        var resultUserDTO = await sut.Invite(userDTO);

        // Assert
        Assert.NotNull(resultUserDTO);
        Assert.Equal(userDTO.Email, resultUserDTO.UserName);
        Assert.Equal(AccountStatus.Invited, resultUserDTO.AccountStatus);
    }

    [Fact]
    public async Task Invite_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);
        var deletedUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        deletedUser.AccountStatus = AccountStatus.Deleted;
        await userManager.CreateAsync(deletedUser);
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var deletedUserDTO = Mapper.Map<UserDTO>(deletedUser);

        // Act
        var deletedUserException = await Assert.ThrowsAsync<BusinessException>(() => sut.Invite(deletedUserDTO));
        deletedUser = ctx.Set<User>().Single(u => u.Email.Equals(deletedUser.Email));
        deletedUser.AccountStatus = AccountStatus.Active;
        deletedUserDTO = Mapper.Map<UserDTO>(deletedUser);
        await ctx.SaveChangesAsync();
        var existingUserException = await Assert.ThrowsAsync<BusinessException>(() => sut.Invite(deletedUserDTO));

        // Assert
        Assert.Equal(UserService.ErrorMessages.EmailExistForDeleted, deletedUserException.Message);
        Assert.Equal(UserService.ErrorMessages.EmailExist, existingUserException.Message);
    }

    // [Fact] //userManager throws: Value cannot be null or empty. (Parameter 'normalizedRoleName') on RemoveFromRoleAsync()
    public async Task ReplaceUsersRolesTest()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        await rolesManager.CreateAsync(new Role("user1 role"));
        await rolesManager.CreateAsync(new Role("user2 role"));
        await rolesManager.CreateAsync(new Role("user3 role"));

        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));
        var user1Role = ctx.Set<Role>().Single(r => r.Name.Equals("user1 role"));
        var user2Role = ctx.Set<Role>().Single(r => r.Name.Equals("user2 role"));
        var user3Role = ctx.Set<Role>().Single(r => r.Name.Equals("user3 role"));
        var user1 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user1.UserRoles.Add(new UserRole { User = user1, Role = commonRole });
        var user2 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user2.UserRoles.Add(new UserRole { User = user2, Role = commonRole });
        var user3 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user3.UserRoles.Add(new UserRole { User = user3, Role = commonRole });

        var sut = CreateUserDataService(ctx);
        await SutDataHelper.InsertData(ctx, user1, user2, user3);

        var userIds = ctx.Set<User>().Select(u => new { u.Id, u.Email });
        var removeSharedRoleDTO = new UsersRolesReplacementDTO
        {
            UsersIds = userIds.Select(u => u.Id).ToList(),
            RolesIdsToAdd = new List<string>(),
            RolesIdsToRemove = new List<string> { commonRole.Id },
        };
        var addRoleToUser1 = new UsersRolesReplacementDTO
        {
            UsersIds = userIds.Where(u => u.Email.Equals(user1.Email)).Select(u => u.Id).ToList(),
            RolesIdsToAdd = new List<string> { user1Role.Id },
            RolesIdsToRemove = new List<string>(),
        };
        var addRoleToUser2 = new UsersRolesReplacementDTO
        {
            UsersIds = userIds.Where(u => u.Email.Equals(user2.Email)).Select(u => u.Id).ToList(),
            RolesIdsToAdd = new List<string> { user2Role.Id },
            RolesIdsToRemove = new List<string>(),
        };
        var addRoleToUser3 = new UsersRolesReplacementDTO
        {
            UsersIds = userIds.Where(u => u.Email.Equals(user3.Email)).Select(u => u.Id).ToList(),
            RolesIdsToAdd = new List<string> { user3Role.Id },
            RolesIdsToRemove = new List<string>(),
        };

        // Act
        var result1 = await sut.ReplaceUsersRoles(removeSharedRoleDTO);
        var result2 = await sut.ReplaceUsersRoles(addRoleToUser1);
        var result3 = await sut.ReplaceUsersRoles(addRoleToUser2);
        var result4 = await sut.ReplaceUsersRoles(addRoleToUser3);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.NotNull(result4);
        Assert.Contains(user1.UserRoles, r => r.RoleId == user1Role.Id);
        Assert.Contains(user2.UserRoles, r => r.RoleId == user2Role.Id);
        Assert.Contains(user3.UserRoles, r => r.RoleId == user3Role.Id);
    }

    [Fact]
    public async Task ReplaceUsersRoles_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        await rolesManager.CreateAsync(new Role("user1 role"));

        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));
        var user1Role = ctx.Set<Role>().Single(r => r.Name.Equals("user1 role"));

        var user1 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user1.UserRoles.Add(new UserRole { User = user1, Role = commonRole });

        await SutDataHelper.InsertData(ctx, user1);

        var userIds = ctx.Set<User>().Select(u => new { u.Id, u.Email });

        var sut = CreateUserDataService(ctx);
        var modelHashingService = new ModelHashingService();
        modelHashingService.Register(Mapper, ctx);
        var dto = new UsersRolesReplacementDTO
        {
            UsersIds = new List<string> { string.Empty },
            RolesIdsToAdd = new List<string>(),
            RolesIdsToRemove = new List<string>(),
        };
        var addRoleToUser1 = new UsersRolesReplacementDTO
        {
            UsersIds = userIds.Where(u => u.Email.Equals(user1.Email)).Select(u => u.Id).ToList(),
            RolesIdsToAdd = new List<string> { user1Role.Id },
            RolesIdsToRemove = new List<string>(),
        };

        // Act
        var resultException = await Assert.ThrowsAsync<UserNotExistsException>(() => sut.ReplaceUsersRoles(dto));
        var test = await sut.ReplaceUsersRoles(addRoleToUser1);

        // Assert
        Assert.NotNull(test);
        Assert.Equal(UserDataService.ErrorMessages.UserNotExistForId.Replace("userId", string.Empty), resultException.Message);
    }

    [Fact]
    public async Task LoginInfo_User_Is_Null()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.LockoutEnabled = false;
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });

        await SutDataHelper.InsertData(ctx, userMapper);

        Func<Task> result = () => service.Login(loginDto, CancellationToken.None);

        await Assert.ThrowsAsync<WrongCredentialsException>(result);
    }

    [Fact]
    public async Task Check_If_LoignDto_And_User_Passwords_Are_Not_Equal()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        userDto.Email = "test@mail.com";
        userDto.Password = "password";
        userDto.LockoutEnabled = false;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.LockoutEnabled = false;
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });
        userMapper.Email = loginDto.Email;

        await userManager.CreateAsync(userMapper, "password2");

        Func<Task> result = () => service.Login(loginDto, CancellationToken.None);

        await Assert.ThrowsAsync<WrongCredentialsException>(result);
        Assert.NotNull(userMapper);
        Assert.IsType<User>(userMapper);
    }

    [Fact]
    public async Task Check_If_LoignDto_And_User_Passwords_Are_Equal()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        userDto.Email = "test@mail.com";
        userDto.Password = SampleUserPassword;
        userDto.LockoutEnabled = false;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.LockoutEnabled = false;
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });
        userMapper.Email = loginDto.Email;
        userMapper.NormalizedUserName = "test@mail.com";

        await userManager.CreateAsync(userMapper, loginDto.Password);

        await service.Login(loginDto, CancellationToken.None);

        userMapper.AccountStatus = AccountStatus.Invited;
        await userManager.UpdateAsync(userMapper);

        Func<Task> accountInvitedError = () => service.Login(loginDto, CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(accountInvitedError);

        Assert.NotNull(userMapper);
        Assert.IsType<User>(userMapper);
        Assert.Equal(SampleUserPassword, loginDto.Password);
    }

    [Fact]
    public async Task Validating_The_Account_Status_Is_Unapproved_While_Login()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        userDto.Email = "test@mail.com";
        userDto.Password = "password";
        userDto.LockoutEnabled = false;
        userDto.TwoFactorEnabled = false;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.LockoutEnabled = false;
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });
        userMapper.Email = loginDto.Email;
        userMapper.NormalizedUserName = "test@mail.com";

        await userManager.CreateAsync(userMapper, loginDto.Password);

        await service.Login(loginDto, CancellationToken.None);

        userMapper.AccountStatus = AccountStatus.Unapproved;
        await userManager.UpdateAsync(userMapper);

        Func<Task> accountUnapprovedError = () => service.Login(loginDto, CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(accountUnapprovedError);

        Assert.NotNull(userMapper);
        Assert.IsType<User>(userMapper);
        Assert.Equal("password", loginDto.Password);
    }

    [Fact]
    public async Task Validating_The_Account_Status_Is_Unverified_While_Login()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        userDto.Email = "test@mail.com";
        userDto.Password = "password";
        userDto.LockoutEnabled = false;
        userDto.TwoFactorEnabled = false;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.LockoutEnabled = false;
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });
        userMapper.Email = loginDto.Email;
        userMapper.NormalizedUserName = "test@mail.com";

        await userManager.CreateAsync(userMapper, loginDto.Password);

        await service.Login(loginDto, CancellationToken.None);

        userMapper.AccountStatus = AccountStatus.Unverified;
        await userManager.UpdateAsync(userMapper);

        Func<Task> accountUnverifiedError = () => service.Login(loginDto, CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(accountUnverifiedError);

        Assert.NotNull(userMapper);
        Assert.IsType<User>(userMapper);
        Assert.Equal("password", loginDto.Password);
    }

    [Fact]
    public async Task Validating_The_Account_Status_Is_Suspended_While_Login()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        userDto.Email = "test@mail.com";
        userDto.Password = "password";
        userDto.LockoutEnabled = false;
        userDto.TwoFactorEnabled = false;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.LockoutEnabled = false;
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });
        userMapper.Email = loginDto.Email;
        userMapper.NormalizedUserName = "test@mail.com";

        await userManager.CreateAsync(userMapper, loginDto.Password);

        await service.Login(loginDto, CancellationToken.None);

        userMapper.AccountStatus = AccountStatus.Suspended;
        await userManager.UpdateAsync(userMapper);

        Func<Task> accountSuspendedError = () => service.Login(loginDto, CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(accountSuspendedError);

        Assert.NotNull(userMapper);
        Assert.IsType<User>(userMapper);
        Assert.Equal("password", loginDto.Password);
    }

    [Fact]
    public async Task Validating_The_Account_Status_Is_Deleted_While_Login()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        userDto.Email = "test@mail.com";
        userDto.Password = "password";
        userDto.LockoutEnabled = false;
        userDto.TwoFactorEnabled = false;

        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        var userMapper = Mapper.Map<User>(userDto);
        userMapper.LockoutEnabled = false;
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });
        userMapper.Email = loginDto.Email;
        userMapper.NormalizedUserName = "test@mail.com";

        await userManager.CreateAsync(userMapper, loginDto.Password);

        await service.Login(loginDto, CancellationToken.None);

        userMapper.AccountStatus = AccountStatus.Deleted;
        await userManager.UpdateAsync(userMapper);

        Func<Task> accountDeletedError = () => service.Login(loginDto, CancellationToken.None);

        await Assert.ThrowsAsync<BusinessException>(accountDeletedError);

        Assert.NotNull(userMapper);
        Assert.IsType<User>(userMapper);
        Assert.Equal("password", loginDto.Password);
    }

    [Fact]
    public async Task Login_Tests()
    {
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var service = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();
        var loginDto = FakeLoginDTO();

        var rolesManager = ServicesFactory.GetRoleManager(ctx as DataContext);
        await rolesManager.CreateAsync(new Role("common"));
        var commonRole = ctx.Set<Role>().Single(r => r.Name.Equals("common"));

        userDto.Email = "test@mail.com";

        var userMapper = Mapper.Map<User>(userDto);

        userMapper.LockoutEnabled = true;
        userMapper.LockoutEnd = DateTime.UtcNow.AddDays(2);
        userMapper.AccountStatus = AccountStatus.Active;
        userMapper.UserRoles.Add(new UserRole { User = userMapper, Role = commonRole });
        userMapper.UserPermissions = new List<UserPermission>()
            {
                    new UserPermission() { UserId = userMapper.Id, Permission = new () { Name = "Perm1" }, User = userMapper },
                    new UserPermission() { UserId = userMapper.Id, Permission = new () { Name = "Perm2" }, User = userMapper },
                    new UserPermission() { UserId = userMapper.Id, Permission = new () { Name = "Perm3" }, User = userMapper },
                    new UserPermission() { UserId = userMapper.Id, Permission = new () { Name = "Perm4" }, User = userMapper },
            };

        ctx.Set<User>().Add(userMapper);
        ctx.SaveChanges();

        await service.Login(loginDto, CancellationToken.None);

        var twoFactorSignIn = await service.TwoFactorAuthenticatorSignIn("testSignIn");
        var twoFactorRecoverySignIn = await service.TwoFactorRecoveryCodeSignIn("testSignIn");
        await service.Logout();

        Assert.Null(twoFactorSignIn);
        Assert.Null(twoFactorRecoverySignIn);
    }

    [Fact]
    public async Task Generate_U2F_Device_Authentication_Challenges_Test()
    {
        // Arrange
        var userDto = ServicesFactory.GetUserEntity();
        var user = Mapper.Map<User>(userDto);
        user.AuthenticationRequests = new List<AuthenticationRequest>()
                {
                    new AuthenticationRequest() { Id = 1, AppId = "1", Challenge = "test", KeyHandle = "testkeyhandle", Version = "1" },
                    new AuthenticationRequest() { Id = 2, AppId = "2", Challenge = "test2", KeyHandle = "testkey2handle", Version = "2" },
                };

        var handle = "testkeyhandle";
        var keyHandle = Encoding.ASCII.GetBytes(handle);
        user.DeviceRegistrations = new List<Device>()
            {
                new Device
                {
                    KeyHandle = keyHandle,
                    AttestationCert = Array.Empty<byte>(),
                    PublicKey = Array.Empty<byte>(),
                }
            };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var appUrl = "appUrl";

        // Act
        var challenges = await sut.GenerateU2FDeviceAuthenticationChallenges(user.Id, appUrl, CancellationToken.None);

        // Assert
        Assert.Single(challenges);
        Assert.Single(user.AuthenticationRequests);
    }

    [Fact]
    public async Task Authenticate_U2FDevice_Test()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.Id = "1";

        var handle = Encoding.UTF8.GetBytes("testkeyhandle").ByteArrayToBase64String();
        user.AuthenticationRequests = new List<AuthenticationRequest>()
            {
                new AuthenticationRequest() { Id = 1, AppId = "1", Challenge = "test", KeyHandle = handle, Version = "1" },
            };

        var keyHandle = handle.Base64StringToByteArray();
        user.DeviceRegistrations = new List<Device>()
            {
                new Device
                {
                    KeyHandle = keyHandle,
                    AttestationCert = Array.Empty<byte>(),
                    PublicKey = Array.Empty<byte>(),
                },
            };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var authenticationResponseDTO = new U2FAuthenticationResponseDTO()
        {
            UserId = "1",
            ClientData = "testdata",
            KeyHandle = handle,
            SignatureData = "Signature",
        };

        // Act, Assert
        await Assert.ThrowsAsync<U2fException>(() => sut.AuthenticateU2FDevice(authenticationResponseDTO, CancellationToken.None));
    }

    [Fact]
    public async Task ReplaceUsersGroupsTest()
    {
        // Arrange
        var sharedGroup = new Group { Name = "Shared" };
        var user1Group = new Group { Name = "user 1" };
        var user1 = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user1.UserGroups = new List<UserGroup> { new UserGroup { Group = sharedGroup, User = user1 } };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user1 });
        await SutDataHelper.InsertData(ctx, user1Group);

        var sut = CreateUserDataService(ctx);

        var modelHashingService = new ModelHashingService();
        modelHashingService.Register(Mapper, ctx);
        var dto = new UsersGroupsReplacementDTO
        {
            GroupsIdsToAdd = new List<string> { modelHashingService.HashProperty(typeof(GroupDTO), "Id", user1Group.Id) },
            GroupsIdsToRemove = new List<string> { modelHashingService.HashProperty(typeof(GroupDTO), "Id", sharedGroup.Id) },
            UsersIds = new List<string> { user1.Id },
        };

        // Act
        var result = await sut.ReplaceUsersGroups(dto);

        // Assert
        Assert.Contains(result.First().Groups, g => g.Id == user1Group.Id);
        Assert.DoesNotContain(result.First().Groups, g => g.Id == sharedGroup.Id);
    }

    [Fact]
    public async Task ReplaceUsersGroups_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = CreateUserDataService(ctx);
        var dto = new UsersGroupsReplacementDTO
        {
            GroupsIdsToAdd = new List<string> { string.Empty },
            GroupsIdsToRemove = new List<string> { string.Empty },
            UsersIds = new List<string> { string.Empty },
        };

        // Act
        var resultException = await Assert.ThrowsAsync<UserNotExistsException>(() => sut.ReplaceUsersGroups(dto));

        // Assert
        Assert.Equal(UserDataService.ErrorMessages.UserNotExistForId.Replace("userId", string.Empty), resultException.Message);
    }

    [Fact]
    public async Task RecoveryLoginTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.RecoveryCode = "recover";
        user.TwoFactorEnabled = true;
        user.U2fEnabled = true;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var dto = new RecoveryCodeDTO
        {
            UserId = user.Id,
            Code = user.RecoveryCode,
        };

        // Act
        var exception = await Record.ExceptionAsync(() => sut.Login(dto));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Get_Types_With_Attribute_Test()
    {
        var assembly = new Mock<Assembly>();
        var service = Common.GetTypesWithAttribute<Attribute>(assembly.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public async Task RecoveryLogin_Exceptions_Test()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.RecoveryCode = "recover";

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var userNotExistDTO = new RecoveryCodeDTO { UserId = string.Empty };
        var loginFailedDTO = new RecoveryCodeDTO
        {
            UserId = user.Id,
            Code = "wrong",
        };

        // Act
        var userNotExistException = await Assert.ThrowsAsync<UserNotExistsException>(() => sut.Login(userNotExistDTO));
        var loginFailedException = await Assert.ThrowsAsync<LoginFailedException>(() => sut.Login(loginFailedDTO));

        // Assert
        Assert.NotNull(userNotExistException);
        Assert.Equal(UserService.ErrorMessages.WrongRecoveryCode, loginFailedException.Message);
    }

    [Fact]
    public async Task ActivateTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);

        user.InvitationToken = new ActivationToken
        {
            Token = await userManager.GenerateUserInviteTokenAsync(user),
            ExpirationDate = DateTime.Today.AddDays(1),
        };
        user.AccountStatus = AccountStatus.Invited;

        var sut = GetService(ctx, userManager);

        var dto = new ResetPasswordDTO
        {
            Email = user.Email,
            Code = user.InvitationToken.Token,
            Password = "newPassword",
        };

        // Act
        var exception = await Record.ExceptionAsync(() => sut.Activate(dto));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task Activate_Exceptions_Test()
    {
        // Arrange
        var emtyEmailDTO = new ResetPasswordDTO { Email = string.Empty };
        var userNotExistDTO = new ResetPasswordDTO { Email = "notExist" };
        var invalidCodeUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        invalidCodeUser.InvitationToken = new ActivationToken { Token = "valid" };
        var invalidCodeDTO = new ResetPasswordDTO { Email = invalidCodeUser.Email, Code = "invalid" };
        var expiredInvitationUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        expiredInvitationUser.InvitationToken = new ActivationToken { ExpirationDate = DateTime.Today.AddDays(-1), Token = "valid" };
        var expiredInvitationDTO = new ResetPasswordDTO { Email = expiredInvitationUser.Email, Code = "valid" };
        var activeUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        activeUser.InvitationToken = new ActivationToken { ExpirationDate = DateTime.Today.AddDays(1), Token = "valid" };
        activeUser.AccountStatus = AccountStatus.Active;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(
            new[] { invalidCodeUser, expiredInvitationUser, activeUser });
        var sut = GetService(ctx);

        var activeUserDTO = new ResetPasswordDTO { Email = activeUser.Email, Code = activeUser.InvitationToken.Token };

        // Act
        var emptyEmailException = await Assert.ThrowsAsync<ObjectNotExistsException>(() => sut.Activate(emtyEmailDTO));
        var userNotExistException = await Assert.ThrowsAsync<UserNotExistsException>(() => sut.Activate(userNotExistDTO));
        var invalidCodeException = await Assert.ThrowsAsync<BusinessException>(() => sut.Activate(invalidCodeDTO));
        var expiredInvitationException = await Assert.ThrowsAsync<BusinessException>(() => sut.Activate(expiredInvitationDTO));
        var activeUserException = await Assert.ThrowsAsync<BusinessException>(() => sut.Activate(activeUserDTO));

        // Assert
        Assert.Equal(UserService.ErrorMessages.UndefinedEmail, emptyEmailException.Message);
        Assert.NotNull(userNotExistException);
        Assert.Equal(UserService.ErrorMessages.ActivationCodeInvalid, invalidCodeException.Message);
        Assert.Equal(UserService.ErrorMessages.InvitationExpired, expiredInvitationException.Message);
        Assert.Equal(UserService.ErrorMessages.UserAlreadyActivated, activeUserException.Message);
    }

    [Fact]
    public async Task RecoverPasswordTest()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);
        await userManager.CreateAsync(user);
        user.AccountStatus = AccountStatus.Active;
        var dto = new RecoverPasswordDTO
        {
            Email = user.Email,
        };

        // Act
        var result = await Record.ExceptionAsync(() => sut.RecoverPassword(dto));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RecoverPassword_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var emptyEmailDTO = new RecoverPasswordDTO { Email = string.Empty };
        var userNotExistDTO = new RecoverPasswordDTO { Email = "notexist@email.com" };
        var activeUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        activeUser.AccountStatus = AccountStatus.Invited;
        var userManager = ServicesFactory.GetUserManager(ctx as DataContext);
        await userManager.CreateAsync(activeUser);
        var notActiveUserDTO = new RecoverPasswordDTO { Email = activeUser.Email };

        // Act
        var e1 = await Record.ExceptionAsync(() => sut.RecoverPassword(emptyEmailDTO));
        var e2 = await Record.ExceptionAsync(() => sut.RecoverPassword(notActiveUserDTO));

        // Assert
        Assert.All(new[] { e1, e2 }, e => Assert.Null(e));
    }

    [Fact]
    public async Task EnableU2FTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        var exception = await Record.ExceptionAsync(() => sut.EnableU2F(user.Id));

        // Assert
        Assert.Null(exception);
        Assert.True(user.U2fEnabled);
    }

    [Fact]
    public async Task EnableU2F_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.EnableU2F(string.Empty));
    }

    [Fact]
    public async Task DisableU2FTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.U2fEnabled = true;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        var exception = await Record.ExceptionAsync(() => sut.DisableU2F(user.Id));

        // Assert
        Assert.Null(exception);
        Assert.False(user.U2fEnabled);
    }

    [Fact]
    public async Task DisableU2F_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.DisableU2F(string.Empty));
    }

    [Fact]
    public async Task Generate2FaOrU2FRecoveryCodesTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.TwoFactorEnabled = true;
        user.U2fEnabled = true;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        var result = await sut.Generate2FaOrU2FRecoveryCodes(user.Id, false);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(user.RecoveryCode);
    }

    [Fact]
    public async Task Generate2FaOrU2FRecoveryCodes_Exceptions_Test()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.Generate2FaOrU2FRecoveryCodes(string.Empty, false));
        var disabledException = await Assert.ThrowsAsync<BusinessException>(() => sut.Generate2FaOrU2FRecoveryCodes(user.Id, false));

        // Assert
        Assert.NotNull(disabledException);
        Assert.Equal(UserService.ErrorMessages.Disabled2FAOrU2F, disabledException.Message);
    }

    [Fact]
    public async Task GenerateU2FDeviceRegistrationChallengeTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var appUrl = "appUrl";

        // Act
        var result = await sut.GenerateU2FDeviceRegistrationChallenge(user.Id, appUrl);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(user.AuthenticationRequests);
        Assert.Equal(1, user.AuthenticationRequests.Count);
        Assert.Equal(appUrl, result.AppId);
    }

    [Fact]
    public async Task GenerateU2FDeviceRegistrationChallenge_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var userDto = ServicesFactory.GetUserEntity();

        // Act & Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.GenerateU2FDeviceRegistrationChallenge(string.Empty, string.Empty));
    }

    [Fact]
    public async Task RegisterU2FDeviceTest()
    {
        // Arrange
        var userDto = ServicesFactory.GetUserEntity();
        var user = Mapper.Map<User>(userDto);

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var appUrl = "test";
        await sut.GenerateU2FDeviceRegistrationChallenge(user.Id, appUrl);
        var dto = new U2FRegistrationResponseDTO { ClientData = Base64UrlEncoder.Encode(JsonSerializer.Serialize(new { typ = "navigator.id.finishEnrollment", challenge = user.AuthenticationRequests.Last().Challenge })), RegistrationData = Base64UrlEncoder.Encode(JsonSerializer.Serialize("testRegistrationData")) };

        // Act
        Func<Task> exception = () => sut.RegisterU2FDevice(user.Id, dto);

        // Assert
        await Assert.ThrowsAsync<U2fException>(exception);
        Assert.Equal(1, user.AuthenticationRequests.Count);
        Assert.NotNull(user.DeviceRegistrations);
        Assert.Equal(0, user.DeviceRegistrations.Count);
    }

    [Fact]
    public async Task RegisterU2FDevice_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var userDataService = CreateUserDataService(ctx);
        var sut = GetService(ctx);

        var userDto = ServicesFactory.GetUserEntity();
        userDto.Id = (await userDataService.Create(userDto, CancellationToken.None)).Id;

        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.AuthenticationRequests = new List<AuthenticationRequest>() { };

        await SutDataHelper.InsertData(ctx, user);

        var dto = new U2FRegistrationResponseDTO();
        var dto2 = new U2FRegistrationResponseDTO()
        {
            ClientData = "testData",
            RegistrationData = "RegistrationData",
        };

        // Act
        var noRegisttrationChalengeException = await Assert.ThrowsAsync<BusinessException>(() => sut.RegisterU2FDevice(user.Id, dto));

        Func<Task> test = () => sut.RegisterU2FDevice(user.Id, dto2);

        // Assert
        Assert.Equal(UserService.ErrorMessages.NoU2FChallenges, noRegisttrationChalengeException.Message);
        await Assert.ThrowsAsync<BusinessException>(test);
    }

    [Fact]
    public async Task StopImpersonation_Test()
    {
        var user = ServicesFactory.GetUserEntity();
        var mapper = Mapper.Map<User>(user);

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { mapper });
        var sut = GetService(ctx);

        var mockClaim = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                        new Claim(Model.ClaimTypes.Impersonation.IsImpersonating, bool.FalseString),
                        new Claim(Model.ClaimTypes.Impersonation.OriginalUserId, mapper.Id.ToString()),
            }, "mock"));

        var mockClaim2 = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                        new Claim(Model.ClaimTypes.Impersonation.IsImpersonating, bool.TrueString),
                        new Claim(Model.ClaimTypes.Impersonation.OriginalUserId, mapper.Id.ToString()),
            }, "mock"));

        var mockClaim3 = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                        new Claim(Model.ClaimTypes.Impersonation.IsImpersonating, bool.TrueString),
                        new Claim(Model.ClaimTypes.Impersonation.OriginalUserId, ""),
            }, "mock"));

        var mockClaim4 = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                        new Claim(Model.ClaimTypes.Impersonation.IsImpersonating, bool.TrueString),
                        new Claim(Model.ClaimTypes.Impersonation.OriginalUserId, "7"),
            }, "mock"));

        var test2 = await sut.StopImpersonation(mockClaim2, CancellationToken.None);
        Func<Task> test3 = async () => await sut.StopImpersonation(mockClaim3, CancellationToken.None);
        Func<Task> test4 = async () => await sut.StopImpersonation(mockClaim4, CancellationToken.None);

        Assert.NotNull(mockClaim);
        Assert.NotNull(mockClaim2);
        await Assert.ThrowsAsync<BusinessException>(() => sut.StopImpersonation(mockClaim, CancellationToken.None));
        Assert.NotNull(test2);
        await Assert.ThrowsAsync<ConflictException>(test3);
        await Assert.ThrowsAsync<ConflictException>(test4);
    }

    [Fact]
    public async Task ImpersonateTest()
    {
        // Arrange
        var impersonatingUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        impersonatingUser.UserRoles.Add(new UserRole
        {
            User = impersonatingUser,
            Role = new Role { Name = Core.Roles.SystemAdminRole },
        });
        var impersonatedUser = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(
            new[] { impersonatingUser, impersonatedUser });
        var sut = GetService(ctx);

        // Act
        Func<Task> result = () => sut.Impersonate(impersonatingUser.Id, impersonatedUser.Id, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NullReferenceException>(result);
    }

    [Fact]
    public async Task GetAllUserPermissionsTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        var roleName = "userRole";
        user.UserRoles.Add(new UserRole { User = user, Role = new Role { Name = roleName } });
        var rolePermissionName = "rolePermissionName";
        var userRole = user.UserRoles.First().Role;
        userRole.RolePermissions.Add(new RolePermission { Role = userRole, Permission = new Permission { Name = rolePermissionName } });
        var userPermission = "userPermission";
        user.UserPermissions.Add(new UserPermission { Permission = new Permission { Name = userPermission }, User = user });

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        var result = await sut.GetAllUserPermissions(user.Id, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name.Equals(rolePermissionName));
        Assert.Contains(result, r => r.Name.Equals(userPermission));
    }

    [Fact]
    public async Task CheckBrowserLoginTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.LastLoginBrowserFingerprint = new Faker().Random.AlphaNumeric(20);
        var newFingerPrint = new Faker().Random.AlphaNumeric(20);
        var authResult = new AuthResultDTO { };

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        await sut.CheckBrowserLoginAsync(authResult, user.Email, newFingerPrint, CancellationToken.None);

        // Assert
        Assert.True(authResult.IsNewBrowserLogin);
    }

    [Fact]
    public async Task NotifyUserOnEmailChangedTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.AccountStatus = AccountStatus.Active;
        var oldEmail = "old_" + user.Email;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        await sut.NotifyUserOnEmailChanged(user.Id, user.Email, CancellationToken.None);

        // Assert
        Assert.Equal(AccountStatus.Unverified, user.AccountStatus);
    }

    [Fact]
    public async Task SendTestEmailTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var emailTemplate = new Faker<EmailTemplate>()
            .RuleFor(p => p.Body, s => s.Random.AlphaNumeric(50))
            .RuleFor(p => p.Code, s => $"#{s.Random.Hexadecimal(6).Substring(2)}")
            .RuleFor(p => p.From, s => s.Internet.Email())
            .RuleFor(p => p.Subject, s => s.Random.AlphaNumeric(50))
            .RuleFor(p => p.Title, s => s.Random.AlphaNumeric(50))
            .Generate();
        ctx.Set<EmailTemplate>().Add(emailTemplate);
        ctx.SaveChanges();

        // Act
        var exception = await Record.ExceptionAsync(() => sut.SendTestEmail(user.Email, emailTemplate.Id, null, CancellationToken.None));

        // Assert
        Assert.Null(exception);
    }
}