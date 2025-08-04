using BBWM.Core.Exceptions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace BBWM.Core.Web.CookieAuth.Test;

public class ServicesModuleLinkageTests
{
    public ServicesModuleLinkageTests()
    {
    }

    [Fact]
    public void Configure_Services_Test()
    {
        // Arrange
        var service = new ServicesModuleLinkage();

        var serviceCollection = new Mock<IServiceCollection>();

        CookieAuthSettings settings = new CookieAuthSettings();

        var mock = new Mock<IConfigurationRoot>();

        IConfigurationSection sett = new ConfigurationSection(mock.Object, "/path/test");

        sett.Get<CookieAuthSettings>();
        sett.Value = "cookie-name";

        var configuration = new Mock<IConfiguration>();
        configuration.Setup(p => p.GetSection("CookieAuthSettings")).Returns(sett);

        Action result = () => service.ConfigureServices(serviceCollection.Object, configuration.Object);

        Assert.Throws<EmptyConfigurationSectionException>(result);
    }
}
