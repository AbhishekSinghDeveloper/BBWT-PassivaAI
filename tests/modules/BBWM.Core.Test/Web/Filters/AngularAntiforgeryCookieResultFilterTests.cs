using BBWM.Core.Web.Filters;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.Filters;

public class AngularAntiforgeryCookieResultFilterTests
{
    public AngularAntiforgeryCookieResultFilterTests()
    {
    }

    [Fact]
    public void OnResultExecuting_StateUnderTest_ExpectedBehavior()
    {
        var antiForgeryMock = new Mock<IAntiforgery>();

        var angularAntiForgeryCookieResultFilter = new AngularAntiforgeryCookieResultFilter(antiForgeryMock.Object);

        var filterMetaData = new Mock<IList<IFilterMetadata>>();
        var actionResult = new Mock<IActionResult>();

        var actionContext = new ActionContext();
        actionContext.HttpContext = new DefaultHttpContext();
        actionContext.RouteData = new Microsoft.AspNetCore.Routing.RouteData();
        actionContext.ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor();

        var resultExecutingContextMock = new ResultExecutingContext(actionContext, filterMetaData.Object, actionResult.Object, new object());
        resultExecutingContextMock.HttpContext = new DefaultHttpContext();

        var viewResult = new ViewResult();
        viewResult.ExecuteResultAsync(actionContext);

        angularAntiForgeryCookieResultFilter.OnResultExecuting(resultExecutingContextMock);

        Assert.NotNull(angularAntiForgeryCookieResultFilter);
        Assert.NotNull(resultExecutingContextMock);
    }
}
