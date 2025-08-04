using BBWM.Core.Web.ModelBinders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System.Text.Json.Nodes;
using Xunit;

namespace BBWM.Core.Test.Web.ModelBinders;

public class FormDataJsonBinderTests
{
    [Fact]
    public async Task BindModelAsync_Should_Throw_On_Missing_Context()
    {
        // Arrange
        var service = new FormDataJsonBinder();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.BindModelAsync(null));
    }

    [Theory]
    [InlineData(null, "{}")]
    [InlineData("test", "")]
    public async Task BindModelAsync_Should_Be_Noop(string fieldName, string fieldValueProvided)
    {
        // Arrange
        const string ModelValue = "This should be left untouched";
        var bindingResult = ModelBindingResult.Success(ModelValue);
        var modelBindingContext = CreateModelBindingContext(fieldName, fieldValueProvided, br => bindingResult = br);

        var binder = new FormDataJsonBinder();

        // Act
        await binder.BindModelAsync(modelBindingContext);

        // Assert
        Assert.True(bindingResult.IsModelSet);
        Assert.Equal(ModelValue, bindingResult.Model);
    }

    [Fact]
    public async Task BindModelAsync_Should_Fail_Binding()
    {
        // Arrange
        var bindingResult = ModelBindingResult.Success("Too bad it'll fail 😢");
        var modelBindingContext = CreateModelBindingContext("test", "{\"hello\": ", br => bindingResult = br);

        var binder = new FormDataJsonBinder();

        // Act
        await binder.BindModelAsync(modelBindingContext);

        // Assert
        Assert.False(bindingResult.IsModelSet);
        Assert.Null(bindingResult.Model);
    }

    [Fact]
    public async Task BindModelAsync_Should_Succeed_Binding()
    {
        // Arrange
        var bindingResult = ModelBindingResult.Failed();
        var modelBindingContext = CreateModelBindingContext(
            "test", "{\"hello\": \"world!\"}", br => bindingResult = br);

        var binder = new FormDataJsonBinder();

        // Act
        await binder.BindModelAsync(modelBindingContext);

        // Assert
        Assert.True(bindingResult.IsModelSet);
        var jobject = Assert.IsType<JsonObject>(bindingResult.Model);

        Assert.Single(jobject.AsEnumerable());
        Assert.True(jobject.TryGetPropertyValue("hello", out JsonNode node));
        Assert.Equal("world!", node.GetValue<string>());
    }

    private static ModelBindingContext CreateModelBindingContext(
        string fieldName, string fieldValueProvided, Action<ModelBindingResult> setBindingResult)
    {
        var modelBindingContext = new Mock<ModelBindingContext>();
        var modelState = new Mock<ModelStateDictionary>();

        if (string.IsNullOrEmpty(fieldName))
        {
            modelBindingContext
                .Setup(p => p.ValueProvider.GetValue(It.IsAny<string>()))
                .Returns(ValueProviderResult.None);
        }
        else
        {
            modelBindingContext.Setup(p => p.FieldName).Returns(fieldName);
            modelBindingContext
                .Setup(p => p.ValueProvider.GetValue(fieldName))
                .Returns(new ValueProviderResult(fieldValueProvided));
        }

        modelBindingContext.Setup(p => p.ModelState).Returns(modelState.Object);

        modelBindingContext
            .SetupSet(p => p.Result = It.IsAny<ModelBindingResult>())
            .Callback(setBindingResult);

        return modelBindingContext.Object;
    }
}