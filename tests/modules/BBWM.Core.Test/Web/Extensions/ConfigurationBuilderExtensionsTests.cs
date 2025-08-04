using BBWM.Core.Web.Extensions;

using Microsoft.Extensions.Configuration;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.Extensions;

public class ConfigurationBuilderExtensionsTests
{
    public ConfigurationBuilderExtensionsTests()
    {
    }

    [Fact]
    public void Add_Eb_Config_Test()
    {
        var configBuilder = new Mock<IConfigurationBuilder>();

        string environmentName = "ASPNETCORE_ENVIRONMENT";

        var configBuilderExtension = ConfigurationBuilderExtensions.AddEbConfig(configBuilder.Object, out environmentName);
    }
}
