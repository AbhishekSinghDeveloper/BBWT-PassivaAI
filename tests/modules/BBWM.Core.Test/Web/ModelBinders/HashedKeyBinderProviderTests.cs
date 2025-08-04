using BBWM.Core.Web.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.ModelBinders;

public class HashedKeyBinderProviderTests
{
    private readonly ModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();

    [Theory]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(string))]
    [InlineData(typeof(HashedKeyBinderProviderTests))]
    public void GetBinder_Should_Be_Null(Type modelType)
    {
        // Arrange
        ModelBinderProviderContext binderProviderContext = CreateModelBinderProviderContext(modelType);
        HashedKeyBinderProvider provider = new();

        // Act
        var binder = provider.GetBinder(binderProviderContext);

        // Assert
        Assert.Null(binder);
    }

    [Theory]
    [InlineData(typeof(int))]
    public void GetBinder_Should_Get_Binder(Type modelType)
    {
        // Arrange
        ModelBinderProviderContext binderProviderContext = CreateModelBinderProviderContext(modelType);
        HashedKeyBinderProvider provider = new();

        // Act
        var binder = provider.GetBinder(binderProviderContext);

        // Assert
        Assert.NotNull(binder);
    }

    private ModelBinderProviderContext CreateModelBinderProviderContext(Type modelType)
    {
        Mock<ModelBinderProviderContext> binderProviderContext = new();
        binderProviderContext
            .Setup(c => c.Metadata)
            .Returns(metadataProvider.GetMetadataForType(modelType));

        return binderProviderContext.Object;
    }
}
