using BBWM.Core.Web.ModelBinders;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.ModelBinders;

public class FormDataJsonBinderProviderTests
{
    private static readonly ModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();

    [Fact]
    public void GetBinder_Should_Throw_Exception_On_Missing_Context()
    {
        // Arrange
        FormDataJsonBinderProvider provider = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
    }

    [Theory]
    [MemberData(nameof(ProviderShouldBeNullTestData))]
    public void GetBinder_Should_Be_Null(ModelMetadata metadata)
    {
        // Arrange
        Mock<ModelBinderProviderContext> binderProviderContext = new();
        binderProviderContext.Setup(c => c.Metadata).Returns(metadata);
        FormDataJsonBinderProvider provider = new();

        // Act
        var binder = provider.GetBinder(binderProviderContext.Object);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_Should_Return_Binder()
    {
        // Arrange
        Mock<ModelBinderProviderContext> binderProviderContext = new();
        binderProviderContext
            .Setup(c => c.Metadata)
            .Returns(
                metadataProvider.GetMetadataForProperty(
                    typeof(MyBindingContainer).GetProperty(nameof(MyBindingContainer.ValidBinding)),
                    typeof(MyBindingModel)));
        FormDataJsonBinderProvider provider = new();

        // Act
        var binder = provider.GetBinder(binderProviderContext.Object);

        // Assert
        Assert.NotNull(binder);
    }

    public static IEnumerable<object[]> ProviderShouldBeNullTestData => new[]
    {
            // Do not bind simple values
            new object[] { metadataProvider.GetMetadataForType(typeof(int)) },

            // Do not bind if target isn't a property
            new object[] { metadataProvider.GetMetadataForType(typeof(MyBindingModel)) },
            new object[]
            {
                metadataProvider.GetMetadataForProperty(
                    typeof(MyBindingModel).GetProperty("Name"), typeof(MyBindingModel)),
            },

            // Do not bind if the property doesn't have the FromFormAttribute
            new object[]
            {
                metadataProvider.GetMetadataForProperty(
                        typeof(MyBindingContainer).GetProperty(nameof(MyBindingContainer.InvalidBindingOne)),
                        typeof(MyBindingModel)),
            },

            // Do not bind if the property is a form file 
            new object[]
            {
                metadataProvider.GetMetadataForProperty(
                    typeof(MyBindingContainer).GetProperty(nameof(MyBindingContainer.InvalidBindingTwo)),
                    typeof(MyBindingModel)),
            },
        };

    private class MyBindingContainer
    {
        public MyBindingModel InvalidBindingOne { get; set; }

        public IFormFile InvalidBindingTwo { get; set; }

        [FromForm]
        public MyBindingModel ValidBinding { get; set; }
    }

    private class MyBindingModel
    {
        public string Name { get; set; }
    }
}
