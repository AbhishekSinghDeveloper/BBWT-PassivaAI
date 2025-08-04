using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Web.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.ModelBinders;

public class FilterInfoModelBinderTests
{
    [Fact]
    public async Task BindModelAsync_Should_Throw_Exception_On_Missing_Context()
    {
        // Arrange
        var service = GetService(default);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.BindModelAsync(null));
    }

    [Theory]
    [InlineData(null, "string")]
    [InlineData("test", "string2")]
    public async Task BindModelAsync_Should_Fail_Binding(string modelName, string filterNameProvided)
    {
        // Arrange
        var service = GetService(default);
        var bindingResult = ModelBindingResult.Success("Too bad it'll fail 😢");

        var modelBindingContext = CreateModelBindingContext(
            modelName, filterNameProvided, () => bindingResult, br => bindingResult = br);

        // Act
        await service.BindModelAsync(modelBindingContext);

        // Assert
        Assert.False(bindingResult.IsModelSet);
        Assert.Null(bindingResult.Model);
    }

    [Theory]
    [MemberData(nameof(BindingShouldSucceedTestData))]
    public async Task BindModelAsync_Should_Succeed_Binding(object model, Action<ModelBindingResult> asserts)
    {
        // Arrange
        var service = GetService(model);
        var bindingResult = ModelBindingResult.Failed();

        var modelBindingContext = CreateModelBindingContext("test", "string", () => bindingResult, br => bindingResult = br);

        // Act
        await service.BindModelAsync(modelBindingContext);

        // Assert
        asserts?.Invoke(bindingResult);
    }

    public static IEnumerable<object[]> BindingShouldSucceedTestData => new[]
    {
            new object[]
            {
                "Hurray!",
                new Action<ModelBindingResult>(bindingResult =>
                {
                    Assert.True(bindingResult.IsModelSet);
                    Assert.Equal("Hurray!", bindingResult.Model);
                }),
            },
            new object[]
            {
                new BindingResultModel { Value = string.Empty },
                CreateCustomModelAsserts(string.Empty),
            },
            new object[]
            {
                new BindingResultModel { Value = "Hello%20World%21" },
                CreateCustomModelAsserts("Hello World!"),
            },
        };

    private static Action<ModelBindingResult> CreateCustomModelAsserts(string expectedValue)
        => (ModelBindingResult bindingResult) =>
        {
            Assert.True(bindingResult.IsModelSet);
            var result = Assert.IsType<BindingResultModel>(bindingResult.Model);
            Assert.Equal(expectedValue, result.Value);
        };

    private static ModelBindingContext CreateModelBindingContext(
        string modelName,
        string filterNameProvided,
        Func<ModelBindingResult> getBindingResult,
        Action<ModelBindingResult> setBindingResult)
    {
        var modelBindingContext = new Mock<ModelBindingContext>();

        modelBindingContext.Setup(p => p.ModelType).Returns(typeof(FilterInfoBase));

        if (string.IsNullOrEmpty(modelName))
        {
            modelBindingContext
                .Setup(p => p.ValueProvider.GetValue(It.IsAny<string>()))
                .Returns(ValueProviderResult.None);
        }
        else
        {
            modelBindingContext.Setup(p => p.ModelName).Returns(modelName);
            modelBindingContext
                .Setup(p => p.ValueProvider.GetValue($"{modelName}.$type"))
                .Returns(new ValueProviderResult(filterNameProvided));
        }

        modelBindingContext
            .Setup(p => p.EnterNestedScope(
                It.IsAny<ModelMetadata>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns(new ModelBindingContext.NestedScope(modelBindingContext.Object));

        modelBindingContext.SetupGet(m => m.Result).Returns(getBindingResult);
        modelBindingContext
            .SetupSet(ctx => ctx.Result = It.IsAny<ModelBindingResult>())
            .Callback(setBindingResult);

        return modelBindingContext.Object;
    }

    private static FilterInfoModelBinder GetService(object model)
    {
        var mockIModelMetadataProvider = new Mock<IModelMetadataProvider>();

        var binder = new Mock<IModelBinder>();
        binder
            .Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
            .Returns<ModelBindingContext>(ctx =>
            {
                ctx.Result = ModelBindingResult.Success(model);
                return Task.CompletedTask;
            });

        var binders = new Dictionary<string, IModelBinder>
        {
            [typeof(StringFilter).FullName] = binder.Object,
        };

        return new FilterInfoModelBinder(mockIModelMetadataProvider.Object, binders);
    }
}
