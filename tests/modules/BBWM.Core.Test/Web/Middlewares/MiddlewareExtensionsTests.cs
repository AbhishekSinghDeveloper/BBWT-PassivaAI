using BBWM.Core.Web.Middlewares;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace BBWM.Core.Test.Web.Middlewares;

public class MiddlewareExtensionsTests
{
    public MiddlewareExtensionsTests()
    {
    }

    [Fact]
    public void Use_Http_To_Https_Redirect_Middleware_Tests()
    {
        var serviceCollection = new ServiceCollection();
        var applicationBuilder2 = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

        var middlewareExtension = MiddlewareExtensions.UseAddUserToLogsMiddleware(applicationBuilder2);
        var middlewareExtension2 = MiddlewareExtensions.UseErrorHandlingMiddleware(applicationBuilder2);
        var middlewareExtension3 = MiddlewareExtensions.UseHttpToHttpsRedirectMiddleware(applicationBuilder2);

        Assert.NotNull(middlewareExtension);
        Assert.NotNull(middlewareExtension2);
        Assert.NotNull(middlewareExtension3);
    }
}
