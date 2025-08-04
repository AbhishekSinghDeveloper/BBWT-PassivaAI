using Bogus;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace BBWM.JWT.Test;

public class JwtServiceTests
{
    private static JwtService GetService()
    {
        var jwtSettings = new Faker<JwtSettings>();
        jwtSettings.RuleFor(p => p.Key, s => "minimum-thirty-two-characters-required");
        jwtSettings.RuleFor(p => p.Issuer, s => s.Random.AlphaNumeric(7));
        jwtSettings.RuleFor(p => p.Audience, s => s.Random.AlphaNumeric(7));

        var options = new Mock<IOptionsSnapshot<JwtSettings>>();
        options.Setup(p => p.Value).Returns(jwtSettings.Generate());

        return new JwtService(options.Object);
    }

    [Fact]
    public void Generate_Token_Test()
    {
        var service = GetService();
        var result = service.GenerateToken("testUser");
        Assert.NotNull(result);
    }

    [Fact]
    public void Generate_Report_Token_Test()
    {
        var service = GetService();
        var result = service.GenerateReportToken("testUser", "/report/path");
        Assert.NotNull(result);
    }
}
