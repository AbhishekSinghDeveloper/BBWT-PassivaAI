using Bogus;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Moq;

using System.Net;

using Xunit;

namespace BBWM.SystemData.Test;

public class SystemDataServiceTests
{
    public SystemDataServiceTests()
    {
    }

    private SystemDataService GetService()
    {
        var data = new byte[16];
        new Random().NextBytes(data);
        IPAddress iPAddress = new IPAddress(data);

        var fakePath = new Faker().Internet.UrlRootedPath();

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(p => p.HttpContext.User.Identity.Name).Returns("testUser");
        httpContextAccessor.Setup(p => p.HttpContext.Connection.RemoteIpAddress).Returns(iPAddress);
        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.Setup(p => p.WebRootPath).Returns(fakePath.ToLower);

        var productVersionService = new Mock<IProductVersionService>();
        productVersionService.Setup(p => p.GetVersion()).Returns("3.9.0-next-123456-WFsq2d3f2902-bbwt3-3");

        return new SystemDataService(httpContextAccessor.Object, webHostEnvironment.Object, productVersionService.Object, null);
    }

    [Fact]
    public async Task Get_Test()
    {
        var service = GetService();
        var result = await service.GetSystemSummary();

        Assert.NotNull(result);
    }

    [Fact]
    public void Get_Version_Info_Test()
    {
        var service = GetService();
        var result = service.GetVersionInfo();

        Assert.NotEmpty(result.ProductVersion);
    }

    [Fact]
    public void Get_Debug_Info_Test()
    {
        var service = GetService();
        var result = service.GetDebugInfo();

        Assert.NotNull(result);
    }

    [Fact]
    public void Get_Api_Exceptions_Info_Test()
    {
        var service = GetService();
        var result = service.GetApiExceptions();

        Assert.NotNull(result);
    }
}
