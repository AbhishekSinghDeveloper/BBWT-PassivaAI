using Autofac;

using BBWM.Core.Exceptions;
using BBWM.Core.Membership.ModuleLinkage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace BBWM.Core.Membership.Test.ModuleLinkage;

public class ServiceModuleLinkageTests
{
    public ServiceModuleLinkageTests()
    {
    }

    private static ServiceModuleLinkage GetService()
    {
        return new ServiceModuleLinkage();
    }

    [Fact]
    public void Configure_Services_Test()
    {
        var service = GetService();

        var serviceCollection = new Mock<IServiceCollection>();
        var iConfigSettings = new Mock<IConfigurationSection>();
        iConfigSettings.Setup(p => p.Key).Returns("MembershipSettings");

        var config = new Mock<IConfiguration>();
        config.Setup(p => p.GetSection(It.IsAny<string>())).Returns(iConfigSettings.Object);

        Action result = () => service.ConfigureServices(serviceCollection.Object, config.Object);

        Assert.Throws<EmptyConfigurationSectionException>(result);
    }

    [Fact]
    public void Register_Dependencies_Test()
    {
        var service = GetService();

        var register = new ContainerBuilder();

        service.RegisterDependencies(register);
        Action result = () => service.RegisterDependencies(register);

        Assert.NotNull(result);
    }
}
