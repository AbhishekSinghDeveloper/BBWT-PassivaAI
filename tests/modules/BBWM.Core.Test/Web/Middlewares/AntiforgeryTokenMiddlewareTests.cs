using BBWM.Core.Web.Middlewares;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.Middlewares;

public class AntiforgeryTokenMiddlewareTests
{
    public AntiforgeryTokenMiddlewareTests()
    {
    }

    [Fact]
    public async Task Should_Add_Antiforgery_Token()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/api";

        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        var antiforgeryTokenSet = new AntiforgeryTokenSet("requestToken", "cookieToken", "firnFieldName", "headerName");

        var antiforgeryMock = new Mock<IAntiforgery>();
        antiforgeryMock.Setup(p => p.GetAndStoreTokens(ctx)).Returns(antiforgeryTokenSet);

        var antiForgeryTokenMiddleware = new AntiforgeryTokenMiddleware(next, antiforgeryMock.Object, new[] { "/api" });
        await antiForgeryTokenMiddleware.Invoke(ctx);

        var headers = ctx.Response.GetTypedHeaders();
        Assert.Contains("XSRF-TOKEN", headers.SetCookie.Select(c => c.Name));
        Assert.Equal("requestToken", headers.SetCookie.First(c => c.Name == "XSRF-TOKEN").Value);
    }
}
