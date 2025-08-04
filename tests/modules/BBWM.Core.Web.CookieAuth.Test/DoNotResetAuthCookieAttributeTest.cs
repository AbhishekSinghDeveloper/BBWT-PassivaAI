using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Moq;

using Xunit;

namespace BBWM.Core.Web.CookieAuth.Test;

public class DoNotResetAuthCookieAttributeTest
{
    [Fact]
    public void OnResultExecuting()
    {
        // Arrange
        ControllerContext controllerContext = new()
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new(),
            ActionDescriptor = new(),
        };

        ResultExecutingContext executingContext =
            new(controllerContext, new List<IFilterMetadata>(), Mock.Of<IActionResult>(), default);

        DoNotResetAuthCookieAttribute doNotResetAuth = new();

        // Act
        doNotResetAuth.OnResultExecuting(executingContext);

        // Assert
        KeyValuePair<object, object> item = Assert.Single(controllerContext.HttpContext.Items);
        Assert.Equal(DoNotResetAuthCookieAttribute.Name, item.Key);
        bool value = Assert.IsType<bool>(item.Value);
        Assert.True(value);
    }
}
