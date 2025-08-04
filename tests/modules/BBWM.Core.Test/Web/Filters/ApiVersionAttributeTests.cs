using BBWM.Core.Web.Filters;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.Filters;

public class ApiVersionAttributeTests
{
    public ApiVersionAttributeTests()
    {
    }

    [Fact]
    public void On_Action_Executing_Test()
    {
        var actionCtx = new ActionContext();
        actionCtx.HttpContext = new DefaultHttpContext();
        actionCtx.RouteData = new Microsoft.AspNetCore.Routing.RouteData();
        actionCtx.ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor();

        var filterMetadata = new Mock<IList<IFilterMetadata>>();
        var dictionary = new Mock<IDictionary<string, object>>();

        var actionExecutedContext = new ActionExecutedContext(actionCtx, filterMetadata.Object, new object());
        var actionExecutingContext = new ActionExecutingContext(actionCtx, filterMetadata.Object, dictionary.Object, new object());

        var apiVersionAttr = new ApiVersionAttribute(ServicesFactory.GetWebHostEnvironment(false));

        apiVersionAttr.OnActionExecuted(actionExecutedContext);
        Action result = () => apiVersionAttr.OnActionExecuted(actionExecutedContext);
        apiVersionAttr.OnActionExecuting(actionExecutingContext);
        Action result2 = () => apiVersionAttr.OnActionExecuting(actionExecutingContext);

        Assert.NotNull(result);
        Assert.NotNull(result2);
    }
}
