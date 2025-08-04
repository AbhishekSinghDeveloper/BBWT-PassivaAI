using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Moq;

using Xunit;

namespace BBWM.FileStorage.Test;

public class AllowedFormFileFormatsAttributeTests
{
    [Fact]
    public void OnActionExecuted()
    {
        // Arrange, Act & Assert
        AllowedFormFileFormatsAttribute service = new();
        service.OnActionExecuted(new(
            new ActionContext(new DefaultHttpContext(), new(), new()),
            new List<IFilterMetadata>(),
            default));
    }

    [Theory]
    [InlineData("application/json", 0, typeof(UnsupportedMediaTypeResult))]
    [InlineData("application/pdf", 1000, typeof(BadRequestObjectResult))]
    public void OnActionExecuting_Should_Reject(string contentType, long maxFileSize, Type expectedResultType)
    {
        // Arrange
        var (formFileFormatsAttribute, context) = CreateAllowedFormFileFormatsAttr(maxFileSize, contentType, 1500);

        // Act
        formFileFormatsAttribute.OnActionExecuting(context);

        // Assert
        Assert.IsType(expectedResultType, context.Result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public void OnActionExecuting(long maxFileSize)
    {
        // Arrange
        var (formFileFormatsAttribute, context) = CreateAllowedFormFileFormatsAttr(maxFileSize, "application/pdf", 500);

        // Act
        formFileFormatsAttribute.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    private static (AllowedFormFileFormatsAttribute, ActionExecutingContext) CreateAllowedFormFileFormatsAttr(
        long maxFileSizeAllowed, string fileContentType, long fileSize)
    {
        AllowedFormFileFormatsAttribute formFileFormatsAttribute =
            maxFileSizeAllowed == 0 ? new("application/pdf") : new(maxFileSizeAllowed, "application/pdf");

        Mock<IFormFile> formFile = new();
        formFile.Setup(f => f.ContentType).Returns(fileContentType);
        formFile.Setup(f => f.Length).Returns(fileSize);

        IFormFileCollection formFiles = new FormFileCollection()
        { formFile.Object };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Form = new FormCollection(new(), formFiles);

        ActionContext actionContext = new(httpContext, new(), new());
        ActionExecutingContext context = new(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), default);

        return (formFileFormatsAttribute, context);
    }
}
