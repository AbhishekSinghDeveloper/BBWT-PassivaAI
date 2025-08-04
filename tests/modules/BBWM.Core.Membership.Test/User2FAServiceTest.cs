using AutoMapper;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Test;
using BBWM.Core.Test.Fixtures;
using BBWM.Core.Test.Utils;
using BBWM.SystemSettings;
using BBWT.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Encodings.Web;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class User2FAServiceTest : IClassFixture<MappingFixture>
{
    public IMapper Mapper { get; }

    public User2FAServiceTest(MappingFixture mappingFixture)
        => Mapper = mappingFixture.DefaultMapper;

    private IUser2FAService GetService(IDataContext context, UserManager<User> userManager = default)
    {
        if (context is not DataContext ctx)
        {
            throw new InvalidCastException();
        }

        userManager ??= ServicesFactory.GetUserManager(ctx);

        var loginSetting = new Mock<IOptionsSnapshot<UserLoginSettings>>();
        loginSetting.SetupGet(opt => opt.Value).Returns(new UserLoginSettings());

        var httpContextAccessor = Core.Test.ServicesFactory.GetHttpContextAccessor();

        var settingService = new SettingsService(ctx, new SettingsSectionService(), null);

        var loginAuditService = new LoginAuditService(httpContextAccessor, new Mock<ILogger<LoginAuditService>>().Object, ctx);

        return new User2FAService(userManager, UrlEncoder.Default, loginSetting.Object, loginAuditService, settingService);
    }

    [Fact]
    public async Task Get2FAEnablingDataTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.TwoFactorEnabled = true;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        var result = await sut.Get2FAEnablingData(user.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.SharedKey));
        Assert.False(string.IsNullOrEmpty(result.AuthenticatorUri));
    }

    [Fact]
    public async Task Enable2FaTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        var dto = new Enabling2FADTO {
            Code = "Test",
        };

        // Act
        var exception = await Record.ExceptionAsync(() => sut.Enable2FA(dto, user.Id));

        // Assert
        Assert.Null(exception);
        Assert.True(user.TwoFactorEnabled);
    }

    [Fact]
    public async Task Enable2Fa_Exceptions_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);
        var dto = new Enabling2FADTO
        {
            Code = "Test",
        };

        // Act & Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.Enable2FA(dto, string.Empty));
    }


    [Fact]
    public async Task Disable2FaTest()
    {
        // Arrange
        var user = Mapper.Map<User>(ServicesFactory.GetUserEntity());
        user.TwoFactorEnabled = true;

        using var ctx = await SutDataHelper.CreateContextWithData<IDataContext, User>(new[] { user });
        var sut = GetService(ctx);

        // Act
        await Assert.ThrowsAsync<UserNotExistsException>(async () => await sut.Disable2FA(string.Empty, string.Empty));
    }

    [Fact]
    public async Task Disable2Fa_Exception_Test()
    {
        // Arrange
        using var ctx = SutDataHelper.CreateEmptyContext<IDataContext>();
        var sut = GetService(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotExistsException>(() => sut.Disable2FA(string.Empty, string.Empty));
    }
}