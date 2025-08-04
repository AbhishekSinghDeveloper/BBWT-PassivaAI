using BBWM.Core.Filters;
using BBWM.Core.Web.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Moq;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Test.Web.ModelBinders;

public class FilterInfoModelBinderProviderTests
{
    private static readonly ModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();

    [Fact]
    public void GetBinder_Should_Return_Null_On_Ivalid_Model_Type()
    {
        // Arrange
        Mock<ModelBinderProviderContext> providerContext = new();
        providerContext
            .Setup(ctx => ctx.Metadata)
            .Returns(metadataProvider.GetMetadataForType(typeof(FilterInfoModelBinderProviderTests)));
        FilterInfoModelBinderProvider filterInfoProvider = new();

        // Act
        var binder = filterInfoProvider.GetBinder(providerContext.Object);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_Should_Return_Binder()
    {
        // Arrange
        var filtersCount = typeof(FilterInfoBase).Assembly
            .GetTypes()
            .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(FilterInfoBase)) && !t.IsAbstract)
            .Count();

        Mock<IModelMetadataProvider> modelMetadataProvider = new();
        modelMetadataProvider
            .Setup(p => p.GetMetadataForType(It.IsAny<Type>()))
            .Returns((Type modelType) => metadataProvider.GetMetadataForType(modelType));

        Mock<ModelBinderProviderContext> binderProviderContext = new();
        binderProviderContext.Setup(c => c.MetadataProvider).Returns(modelMetadataProvider.Object);
        binderProviderContext.Setup(c => c.Metadata).Returns(metadataProvider.GetMetadataForType(typeof(FilterInfoBase)));

        FilterInfoModelBinderProvider provider = new();

        // Act
        var binder = provider.GetBinder(binderProviderContext.Object);

        // Assert
        Assert.NotNull(binder);
        modelMetadataProvider.Verify(p => p.GetMetadataForType(It.IsAny<Type>()), Times.Exactly(filtersCount));
        binderProviderContext.Verify(c => c.CreateBinder(It.IsAny<ModelMetadata>()), Times.Exactly(filtersCount));
    }
}
