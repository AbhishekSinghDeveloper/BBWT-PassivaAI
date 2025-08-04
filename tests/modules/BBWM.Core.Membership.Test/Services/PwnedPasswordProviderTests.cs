using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Services;
using Moq;

using System.Security.Cryptography;

using Xunit;

namespace BBWM.Core.Membership.Test.Services;

public class PwnedPasswordProviderTests
{
    [Fact]
    public async Task GetPasswordPwned_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var provider = new Mock<IPwnedPasswordProvider>();
        provider.Setup(p => p.GetPasswordPwned(It.IsAny<string>())).Returns(Task.FromResult("PasswordSHA1"));

        byte[] code = new byte[16];
        SHA1Managed sha1 = new SHA1Managed();
        var hash = sha1.ComputeHash(code);
        string pass = Convert.ToBase64String(hash);

        // Assert
        var passwordProvider = new PwnedPasswordProvider();
        var result = await passwordProvider.GetPasswordPwned(pass);

        Assert.NotNull(result);
    }
}
