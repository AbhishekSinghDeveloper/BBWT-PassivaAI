using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Membership.TokenProviders;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using System.Text;

using Xunit;

namespace BBWM.Core.Membership.Test.TokenProviders;

public class PasswordResetTokenProviderTests
{
    [Theory]
    [MemberData(nameof(ValidateAsyncTestData))]
    public async Task ValidateAsync(
        DateTimeOffset tokenCreationTime,
        int? passwordResetExpireInDays,
        int? userInvitationExpireInDays,
        string purpose,
        bool expectedValidationResult)
    {
        // Arrange
        CreateServiceResult createResult =
            await CreateService(tokenCreationTime, purpose, passwordResetExpireInDays, userInvitationExpireInDays);

        // Act
        bool success = await createResult.TokenProvider.ValidateAsync(
            purpose, createResult.TestToken, createResult.UserManager, createResult.User);

        // Assert
        Assert.Equal(expectedValidationResult, success);
    }

    public static IEnumerable<object[]> ValidateAsyncTestData => new[]
    {
            new object[]
            {
                DateTimeOffset.UtcNow.AddDays(-2),          // tokenCreationTime
                1,                                          // passwordResetExpireInDays
                null,                                       // userInvitationExpireInDays
                ResetTokenPurpose.ResetPassword,            // purpose
                false,                                      // expectedValidationResult
            },
            new object[]
            {
                DateTimeOffset.UtcNow.AddHours(-12),
                null,
                null,
                ResetTokenPurpose.ResetPassword,
                true,
            },
            new object[]
            {
                DateTimeOffset.UtcNow.AddDays(-2),
                null,
                1,
                ResetTokenPurpose.UserInvite,
                false,
            },
            new object[]
            {
                DateTimeOffset.UtcNow.AddHours(-12),
                null,
                null,
                ResetTokenPurpose.UserInvite,
                true,
            },
            new object[]
            {
                DateTimeOffset.UtcNow.AddHours(-2),
                null,
                null,
                "Unit Testing",
                true,
            },
        };

    private static async Task<CreateServiceResult> CreateService(
        DateTimeOffset tokenCreationTime,
        string purpose,
        int? passwordResetExpireInDays,
        int? userInvitationExpireInDays)
    {
        Mock<IDataProtector> protector = new(MockBehavior.Strict);
        protector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns((byte[] bytes) => bytes);

        Mock<IDataProtectionProvider> protectionProvider = new(MockBehavior.Strict);
        protectionProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(protector.Object);

        Mock<IOptions<DataProtectionTokenProviderOptions>> providerOptions = new();
        providerOptions.Setup(o => o.Value).Returns(new DataProtectionTokenProviderOptions());

        Mock<ISettingsService> settingsService = new();
        UserPasswordSettings passwordSettings = null;
        RegistrationSettings registrationSettings = null;

        if (passwordResetExpireInDays is not null)
            passwordSettings = new() { PasswordResetTokenExpireInDays = passwordResetExpireInDays.Value };

        if (userInvitationExpireInDays is not null)
            registrationSettings = new() { UserInvitationExpireInDays = userInvitationExpireInDays.Value };

        settingsService.Setup(s => s.GetSettingsSection<UserPasswordSettings>()).Returns(passwordSettings);
        settingsService.Setup(s => s.GetSettingsSection<RegistrationSettings>()).Returns(registrationSettings);

        ILogger<PasswordResetTokenProvider> logger = Mock.Of<ILogger<PasswordResetTokenProvider>>();

        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();

        User user = new() { UserName = "Test User" };
        UserManager<User> userManager = ServicesFactory.GetUserManager(dataContext);
        await userManager.CreateAsync(user);

        return new()
        {
            TokenProvider = new(protectionProvider.Object, providerOptions.Object, settingsService.Object, logger),
            UserManager = userManager,
            DataContext = dataContext,
            User = user,
            TestToken = CreateTestToken(tokenCreationTime, user.Id, purpose, user.SecurityStamp),
        };
    }

    private static string CreateTestToken(DateTimeOffset creationTime, string userId, string purpose, string stamp)
    {
        MemoryStream ms = new();
        using BinaryWriter writer = new(ms, Encoding.UTF8);
        writer.Write(creationTime.Ticks);
        writer.Write(userId);
        writer.Write(purpose);
        writer.Write(stamp);

        return Convert.ToBase64String(ms.ToArray());
    }

    private class CreateServiceResult
    {
        public UserManager<User> UserManager { get; set; }

        public PasswordResetTokenProvider TokenProvider { get; set; }

        public DataContext DataContext { get; set; }

        public User User { get; set; }

        public string TestToken { get; set; }
    }
}
