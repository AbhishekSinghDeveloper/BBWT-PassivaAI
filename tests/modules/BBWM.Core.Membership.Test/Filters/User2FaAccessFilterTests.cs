using BBWM.Core.Membership.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Moq;

using System.Security.Claims;

using Xunit;

namespace BBWM.Core.Membership.Test.Filters;

public class User2FaAccessFilterTests
{
    public User2FaAccessFilterTests()
    {
    }

    private static User2FaAccessFilter GetService()
    {
        var mockLogger = new Mock<ILogger<User2FaAccessFilter>>();

        return new User2FaAccessFilter(
            mockLogger.Object);
    }

    [Fact]
    public async Task On_Authorization_Async_Test()
    {
        var service = GetService();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                       new Claim(ClaimTypes.Name, "examplname"),
                       new Claim(ClaimTypes.NameIdentifier, "1"),
                       new Claim("custom-claim", "example claim value"),
                }, "mock"));

        var httpContext = new DefaultHttpContext() { User = user };

        var actionContext = new ActionContext()
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
            {
                EndpointMetadata = new List<object>() { new AllowAnonymousAttribute(), new IgnoreSetup2FaCheckAttribute() },
            },
        };

        var mock = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>() { });

        await service.OnAuthorizationAsync(mock);

        Assert.Null(mock.Result);
    }
}
