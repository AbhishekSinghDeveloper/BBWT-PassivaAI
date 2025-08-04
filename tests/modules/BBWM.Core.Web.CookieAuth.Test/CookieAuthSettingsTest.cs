using Bogus;

using Xunit;

namespace BBWM.Core.Web.CookieAuth.Test;

public class CookieAuthSettingsTest
{
    public CookieAuthSettingsTest()
    {
    }

    [Fact]
    public void Cookie_Auth_Settings_Test()
    {
        var faker = new Faker<CookieAuthSettings>();
        faker.RuleFor(p => p.CookieName, s => s.Random.AlphaNumeric(7));
        faker.RuleFor(p => p.LoginPath, s => s.Random.AlphaNumeric(7));
        faker.RuleFor(p => p.ApiPath, s => s.Random.AlphaNumeric(7));
        faker.RuleFor(p => p.ExpireTime, s => s.Random.Int());

        faker.Generate();
    }
}
