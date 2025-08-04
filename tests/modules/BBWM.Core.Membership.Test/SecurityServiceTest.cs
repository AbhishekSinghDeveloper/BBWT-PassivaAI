using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Test;
using BBWM.ReCaptcha;
using BBWM.SystemSettings;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static BBWM.Core.Membership.Services.SecurityService;

namespace BBWM.Core.Membership.Test;

public class SecurityServiceTest
{
    private readonly DataContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityServiceTest"/> class.
    /// </summary>
    public SecurityServiceTest()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }

    private ISecurityService GetService(FailedAttemptsPasswordSettings fapSettings = null, UserPasswordSettings userPassSettings = null)
    {
        var userManager = ServicesFactory.GetUserManager(_context);

        var settingService = new Mock<ISettingsService>();
        settingService.Setup(a => a.GetSettingsSection<ReCaptchaSettings>()).Returns(new ReCaptchaSettings());
        settingService.Setup(a => a.GetSettingsSection<FailedAttemptsPasswordSettings>()).Returns(fapSettings);
        settingService.Setup(a => a.GetSettingsSection<UserPasswordSettings>()).Returns(userPassSettings);

        var auditService = new Mock<ILoginAuditService>();
        auditService.Setup(a => a.GetLastAttemptsCountAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(3);

        return new SecurityService(_context, Core.Test.ServicesFactory.GetHttpContextAccessor(), userManager, settingService.Object, auditService.Object);
    }

    [Theory]
    [InlineData(LockType.NeverLock, 5, 5)]
    [InlineData(LockType.NeverLock, 2, 5)]
    [InlineData(LockType.AfterSeveralFailedAttempts, 5, 5)]
    [InlineData(LockType.AfterSeveralFailedAttempts, 2, 0)]
    public async Task Must_Not_Lock_Out_By_Ip(LockType lockType, int maxInvalidPasswordAttempts, int intervalInSeconds)
    {
        // Arrange
        var dbSet = _context.Set<LockedOutIp>();

        var fapSettings = this.GetFailedAttemptsPasswordSettings(lockType, maxInvalidPasswordAttempts, intervalInSeconds);

        var securityService = GetService(fapSettings);
        var ip = new Faker().Internet.Ip();

        // Act
        await securityService.CheckIpLockOut(ip, CancellationToken.None);
        var lockedOutIp = dbSet.FirstOrDefault(l => l.IpAddress == ip);

        // Assert
        Assert.Null(lockedOutIp);
    }

    [Fact]
    public async Task Get_Last_Attmept_Count_Async()
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var logger = new Mock<ILogger<LoginAuditService>>();

        var dateTimeOffset = new DateTimeOffset();

        var auditService = new LoginAuditService(contextAccessor.Object, logger.Object, _context);
        var result = await auditService.GetLastAttemptsCountAsync("1", dateTimeOffset);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Must_Lock_Out_By_Ip()
    {
        // Arrange
        var dbSet = _context.Set<LockedOutIp>();

        var fapSettings = this.GetFailedAttemptsPasswordSettings(LockType.AfterSeveralFailedAttempts, 2);

        var securityService = GetService(fapSettings);
        var ip = new Faker().Internet.Ip();

        // Act
        await securityService.CheckIpLockOut(ip, CancellationToken.None);
        var lockedOutIp = dbSet.FirstOrDefault(l => l.IpAddress == ip);

        // Assert
        Assert.NotNull(lockedOutIp);
    }

    [Fact]
    public async Task Must_Return_Longest_Active_LockingByIp()
    {
        // Arrange
        var dbSet = _context.Set<LockedOutIp>();

        var securityService = GetService(null);
        var ip = new Faker().Internet.Ip();

        var latestDateTime = DateTime.Now.AddDays(20);

        await dbSet.AddAsync(new LockedOutIp() { IpAddress = ip, LockoutEnd = DateTime.Now.AddDays(5) });
        await dbSet.AddAsync(new LockedOutIp() { IpAddress = ip, LockoutEnd = latestDateTime });
        await dbSet.AddAsync(new LockedOutIp() { IpAddress = ip, LockoutEnd = DateTime.Now.AddDays(3) });

        await _context.SaveChangesAsync(CancellationToken.None);

        // Act
        var longestActiveLockingByIp = await securityService.GetLongestActiveLockingByIp(ip, CancellationToken.None);

        // Assert
        Assert.Equal(longestActiveLockingByIp.LockoutEnd, latestDateTime);
    }

    [Fact]
    public async Task Try_Lock_User_On_Invalid_Recaptcha_Test()
    {
        var service = GetService();

        var user = new Faker<User>()
            .RuleFor(p => p.Id, s => null)
            .RuleFor(p => p.FirstName, s => s.Person.FirstName)
            .RuleFor(p => p.LastName, s => s.Person.LastName)
            .RuleFor(p => p.Email, (s, p) => s.Internet.Email(p.FirstName, p.LastName))
            .RuleFor(p => p.UserName, (s, p) => p.Email)
            .RuleFor(p => p.U2fEnabled, s => false)
            .RuleFor(p => p.TwoFactorEnabled, p => false)
            .Generate();

        bool value = true;

        await service.TryLockUserOnInvalidRecaptcha(user, value, CancellationToken.None);
        await service.TryLockUserOnInvalidRecaptcha(user, !value, CancellationToken.None);
    }

    [Fact]
    public async Task Ip_Must_Be_Locked()
    {
        // Arrange
        var securityService = GetService(null);
        var ip = new Faker().Internet.Ip();

        await _context.Set<LockedOutIp>()
            .AddAsync(new LockedOutIp() { IpAddress = ip, LockoutEnd = DateTime.Now.AddDays(20) });

        await _context.SaveChangesAsync(CancellationToken.None);

        // Act
        var isIpLocked = await securityService.IsIpLocked(ip, CancellationToken.None);

        // Assert
        Assert.True(isIpLocked);
    }

    [Fact]
    public async Task Ip_Must_Be_Not_Locked()
    {
        // Arrange
        var securityService = GetService(null);
        var ip = new Faker().Internet.Ip();

        await _context.Set<LockedOutIp>()
            .AddAsync(new LockedOutIp() { IpAddress = ip, LockoutEnd = DateTime.Now.AddDays(-20) });

        await _context.SaveChangesAsync(CancellationToken.None);

        // Act
        var isIpLocked = await securityService.IsIpLocked(ip, CancellationToken.None);

        // Assert
        Assert.False(isIpLocked);
    }

    [Fact]
    public async Task Must_Throw_Conflict_Exception_On_AddFailedAttemptForUser()
    {
        // Arrange
        var fapSettings = this.GetFailedAttemptsPasswordSettings();
        var sut = GetService(fapSettings);

        var user = new User() { LockoutEnabled = true };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        // Assert
        await Assert.ThrowsAsync<ConflictException>(async () => await sut.AddFailedAttemptForUser(user.Id));
    }

    [Fact]
    public async Task Must_Set_Access_Failed_On_AddFailedAttemptForUser()
    {
        // Arrange
        var fapSettings = this.GetFailedAttemptsPasswordSettings(
            lockTypeAccount: LockType.AfterSeveralFailedAttempts,
            maxInvalidPasswordAttempts: 5);
        var sut = GetService(fapSettings);

        var user = new User() { LockoutEnabled = false };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await sut.AddFailedAttemptForUser(user.Id);

        // Assert
        Assert.Equal(1, user.AccessFailedCount);
        Assert.NotNull(user.FirstPasswordFailureDate);
    }

    [Fact]
    public async Task Must_Lock_User_On_AddFailedAttemptForUser()
    {
        // Arrange
        var fapSettings = this.GetFailedAttemptsPasswordSettings(
            lockTypeAccount: LockType.AfterSeveralFailedAttempts,
            maxInvalidPasswordAttempts: 3,
            unlockTypeAccount: UnlockType.Temporary,
            passwordAttemptWindow: 5);
        var sut = GetService(fapSettings);

        var user = new User()
        {
            AccountStatus = BBWM.Core.Membership.Enums.AccountStatus.Active,
            LockoutEnabled = false,
            AccessFailedCount = 3,
            FirstPasswordFailureDate = DateTime.Now.AddMinutes(-2),
        };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await sut.AddFailedAttemptForUser(user.Id);

        // Assert
        Assert.Equal(0, user.AccessFailedCount);
        Assert.True(user.LockoutEnabled);
        Assert.NotNull(user.LockoutEnd);
    }

    [Fact]
    public async Task Must_Unlock_User()
    {
        // Arrange
        var fapSettings = this.GetFailedAttemptsPasswordSettings();
        var sut = GetService(fapSettings);

        var user = new User() { LockoutEnabled = true, FirstPasswordFailureDate = DateTime.Now.AddDays(-1) };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await sut.UnlockUser(user);

        // Assert
        Assert.False(user.LockoutEnabled);
        Assert.Null(user.FirstPasswordFailureDate);
    }

    [Fact]
    public async Task Should_Not_Allow_Set_Password_Like_Email()
    {
        // Arrange
        var userPassSettings = new UserPasswordSettings() { PasswordReuse = PasswordReuseSettings.MayUse };
        var sut = GetService(userPassSettings: userPassSettings);

        var user = new User()
        {
            Email = "Test@gmail.com",
        };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        var newPassword = sut.GetHashedPassword("Test@gmail.com");

        // Act
        var message = sut.CheckUsersNewPassword(user, newPassword);

        // Assert
        Assert.Equal(SecurityErrorMessages.PasswordDifferentFromEmail, message);
    }

    [Fact]
    public async Task Should_Not_Allow_Set_Password_Found_In_PassworkHistory()
    {
        // Arrange
        var sut = GetService();

        var passwordHasher = new PasswordHasher<User>();

        var user = new User() { Email = "Test@gmail.com", };
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();

        var oldPasswordHash = passwordHasher.HashPassword(user, sut.GetHashedPassword("oldpassword"));
        user.PasswordHash = oldPasswordHash;

        var oldPassword = new PasswordHistory
        {
            UserId = user.Id,
            Password = oldPasswordHash,
            CreateDate = DateTimeOffset.Now,
        };
        await _context.Set<PasswordHistory>().AddAsync(oldPassword);
        await _context.SaveChangesAsync();

        var newPassword = sut.GetHashedPassword("oldpassword");

        // Case: Re
        // Act
        var resultMayReUse = GetService(userPassSettings:
                        new UserPasswordSettings()
                        {
                            PasswordReuse = PasswordReuseSettings.MayReUse,
                            LastPasswordsNumber = 1,
                        })
                      .CheckUsersNewPassword(user, newPassword);

        // Assert
        Assert.Equal(SecurityErrorMessages.PasswordMayReUse(1), resultMayReUse);


        var resultMayNeverUse = GetService(userPassSettings:
                        new UserPasswordSettings()
                        {
                            PasswordReuse = PasswordReuseSettings.NeverUse,
                            LastPasswordsNumber = 1,
                        })
                      .CheckUsersNewPassword(user, newPassword);
        // Assert
        Assert.Equal(SecurityErrorMessages.PasswordCannotBeReUsed, resultMayNeverUse);
    }

    private FailedAttemptsPasswordSettings GetFailedAttemptsPasswordSettings(
        LockType lockTypeAccount = LockType.NeverLock,
        int maxInvalidPasswordAttempts = 5,
        int intervalInSeconds = 5,
        int passwordAttemptWindow = 5,
        UnlockType unlockTypeAccount = UnlockType.Temporary)
    {
        return new FailedAttemptsPasswordSettings()
        {
            LockTypeAccount = lockTypeAccount,
            MaxInvalidPasswordAttempts = maxInvalidPasswordAttempts,
            IntervalInSeconds = intervalInSeconds,
            PasswordAttemptWindow = passwordAttemptWindow,
            UnlockTypeAccount = unlockTypeAccount,
        };
    }
}