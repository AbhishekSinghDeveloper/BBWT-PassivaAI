using BBWM.DataProcessing.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using Moq;

using Xunit;

namespace BBWM.DataProcessing.Test.Services;

public class ViewRenderServiceTests
{
    [Fact]
    public async Task RenderToString_Should_Throw_On_Missing_View()
    {
        // Arrange
        var razorEngine = new Mock<IRazorViewEngine>();
        razorEngine
            .Setup(r => r.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(ViewEngineResult.NotFound("test", Array.Empty<string>()));

        var sut = new ViewRenderService(
            razorEngine.Object, Mock.Of<ITempDataProvider>(), Mock.Of<IServiceProvider>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.RenderToString("test", new object()));
    }

    [Fact]
    public async Task RenderToString()
    {
        // Arrange
        var view = new Mock<IView>();
        view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>())).Returns(Task.CompletedTask).Verifiable();

        var viewEngine = ViewEngineResult.Found("test", view.Object);

        var razorEngine = new Mock<IRazorViewEngine>();
        razorEngine
            .Setup(r => r.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(viewEngine);

        var sut = new ViewRenderService(
            razorEngine.Object, Mock.Of<ITempDataProvider>(), Mock.Of<IServiceProvider>());

        // Act
        await sut.RenderToString("test", new object());

        // Assert
        view.Verify();
    }
}
