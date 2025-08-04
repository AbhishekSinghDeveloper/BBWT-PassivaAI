using BBWM.Core.Web.Middlewares;

using Microsoft.AspNetCore.Http;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

using System.Security.Claims;

using Xunit;

using static BBWM.Core.Membership.Model.ClaimTypes;

namespace BBWM.Core.Test.Web.Middlewares;

public class AddUsernameToLogsMiddlewareTests
{
    static readonly RequestDelegate next = hc =>
    {
        Log.Information("Hi there!");
        return Task.CompletedTask;
    };

    public AddUsernameToLogsMiddlewareTests()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich
            .FromLogContext()
            .WriteTo.Sink(new TestCorrelatorSink())
            .CreateLogger();
    }

    [Fact]
    public async Task Should_Add_Username_To_Logs()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var addUserToLogsMiddleware = new AddUsernameToLogsMiddleware(next);

        // Act
        using var testContext = TestCorrelator.CreateContext();
        await addUserToLogsMiddleware.Invoke(httpContext);

        // Assert
        var log = Assert.Single(TestCorrelator.GetLogEventsFromContextGuid(testContext.Guid));
        AssertLogProperties(log, new[] { ("Username", "examplname"), ("UserId", "1") });
    }

    [Fact]
    public async Task Should_Add_Impersonating_Info_To_Logs()
    {
        // Arrange
        var ctx = CreateHttpContext();
        var addUserImpersonationToLogsMiddleware = new AddUserImpersonationToLogsMiddleware(next);

        // Act
        using var testContext = TestCorrelator.CreateContext();
        await addUserImpersonationToLogsMiddleware.Invoke(ctx);

        // Assert
        var log = Assert.Single(TestCorrelator.GetLogEventsFromContextGuid(testContext.Guid));
        AssertLogProperties(log, new[] { ("IsImpersonating", "True"), ("OriginalUserName", "examplname"), ("OriginalUserId", "1") });
    }

    private static HttpContext CreateHttpContext()
    {
        HttpContext ctx = new DefaultHttpContext();

        var claims = new Claim[]
        {
                new Claim(Impersonation.IsImpersonating, "true"),
                new Claim("IsImpersonating", "true"),
                new Claim(ClaimTypes.Name, "examplname"),
                new Claim(Impersonation.OriginalUserName, "examplname"),
                new Claim("OriginalUserName", "examplname"),
                new Claim(ClaimTypes.Authentication, "IsAuthenticated"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
                new Claim(Impersonation.OriginalUserId, "1"),
                new Claim("OriginalUserId", "1"),
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        ctx.User = new ClaimsPrincipal(identity);

        return ctx;
    }

    private void AssertLogProperties(LogEvent log, (string, string)[] pInfo)
    {
        Assert.Equal(pInfo.Length, log.Properties.Count);

        foreach (var (key, value) in pInfo)
        {
            var property = log.Properties[key];
            Assert.Equal(value, property.ToString().Trim('"'));
        }
    }
}
