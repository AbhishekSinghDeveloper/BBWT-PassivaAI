using BBWM.Core.Data;
using BBWM.Core.DTO;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWT.Tests.modules.BBWM.Core.Test.ModelHashing.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Test.Web.ModelBinders;

public class HashedKeyBinderTests
{
    private static bool setBinderModelName = false;

    [Fact]
    public async Task BindModelAsync_Should_Throw_On_Missing_Context()
    {
        // Arrange
        HashedKeyBinder binder = new(Mock.Of<ILoggerFactory>(), Mock.Of<IModelHashingService>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => binder.BindModelAsync(default));
    }

    /*[Theory]
    [MemberData(nameof(BinderShouldSucceedTestData))]
    public async Task BindModelAsync_Should_Succeed_Binding(
        string modelName,
        string modelValueProvided,
        Type baseType,
        Type modelType,
        ModelMetadata modelMetadata,
        IModelHashingService modelHashingService,
        bool forceProvideNoValue,
        object expectedModel)
    {
        // Arrange
        TypeInfo typeInfo = CreateTypeInfo(baseType);
        ControllerContext controllerContext = CreateControllerContext(typeInfo);
        ModelBindingContext modelBindingContext = CreateModelBindingContext(
            modelName, modelValueProvided, controllerContext, modelType, modelMetadata, forceProvideNoValue);
        HashedKeyBinder binder = CreateBinder(modelHashingService);

        // Act
        await binder.BindModelAsync(modelBindingContext);

        // Assert
        Assert.True(modelBindingContext.Result.IsModelSet);
        Assert.Equal(expectedModel, modelBindingContext.Result.Model);
    }

    [Fact]
    public async Task BindModelAsync_Should_Fail_Binding()
    {
        // Arrange
        const string hashedId = "1-1234567890ABCDEF";
        var modelName = nameof(MasterDTO.Id);
        IModelHashingService hashingService = CreateModelHashingService<MasterDTO>(modelName, hashedId, null);
        ControllerContext controllerContext = CreateControllerContext(
            CreateTypeInfo(typeof(DataControllerBase<IEntity<int>, MasterDTO, MasterDTO, int>)));
        ModelBindingContext modelBindingContext = CreateModelBindingContext(modelName, hashedId, controllerContext);
        HashedKeyBinder binder = CreateBinder(hashingService);

        // Act
        await binder.BindModelAsync(modelBindingContext);

        // Assert
        AssertBindingResultNotSet(modelBindingContext.Result);
    }

    [Theory]
    [MemberData(nameof(BinderShouldNoopTestData))]
    public async void BindModelAsync_Should_Be_Noop(
        string modelName,
        string modelValueProvided,
        Type baseType,
        Type modelType,
        ModelMetadata modelMetadata,
        IModelHashingService modelHashingService,
        bool forceProvideNoValue)
    {
        // Arrange
        TypeInfo typeInfo = CreateTypeInfo(baseType);
        ControllerContext controllerContext = CreateControllerContext(typeInfo);
        ModelBindingContext modelBindingContext = CreateModelBindingContext(
            modelName, modelValueProvided, controllerContext, modelType, modelMetadata, forceProvideNoValue);
        HashedKeyBinder binder = CreateBinder(modelHashingService);

        // Act
        await binder.BindModelAsync(modelBindingContext);

        // Assert
        AssertBindingResultNotSet(modelBindingContext.Result);
    }*/

    public static IEnumerable<object[]> BinderShouldSucceedTestData => new[]
    {
            new object[]
            {
                nameof(MasterDTO.Id),                                                   // modelName
                "1",                                                                    // modelValueProvided
                typeof(DataControllerBase<IEntity<int>, MasterDTO, MasterDTO, int>),    // baseType
                null,                                                                   // modelType
                null,                                                                   // modelMetadata
                null,                                                                   // modelHashingService
                false,                                                                  // forceProvideNoValue
                1,                                                                      // expectedModel
            },
            new object[]
            {
                nameof(DetailDTO.MasterId),
                "1",
                typeof(DataControllerBase<IEntity<int>, DetailDTO, DetailDTO, int>),
                null,
                null,
                null,
                false,
                1,
            },
            new object[]
            {
                "somevalue",
                "1",
                typeof(DataControllerBase<IEntity<int>, DetailDTO, DetailDTO, int>),
                typeof(int),
                null,
                null,
                false,
                1,
            },
            new object[]
            {
                nameof(MasterDTO.Id),
                "1",
                typeof(Core.Web.ControllerBase),
                null,
                MetadataForProperty<MasterDTO>(nameof(MasterDTO.Id)),
                null,
                false,
                1,
            },
            new object[]
            {
                nameof(MasterDTO.Id),
                "1",
                typeof(Core.Web.ControllerBase),
                typeof(int),
                MetadataForType<int>(),
                null,
                false,
                1,
            },
            new object[]
            {
                nameof(MasterDTO.Id),
                "1-1234567890ABCDEF",
                typeof(DataControllerBase<IEntity<int>, MasterDTO, MasterDTO, int>),
                null,
                null,
                CreateModelHashingService<MasterDTO>(nameof(MasterDTO.Id), "1-1234567890ABCDEF", 1),
                false,
                1,
            },
            new object[]
            {
                nameof(MyIdBinderDTO.MyNullableInt),
                "1",
                typeof(DataControllerBase<IEntity<int>, MyIdBinderDTO, MyIdBinderDTO, int>),
                null,
                null,
                null,
                false,
                1,
            },
            new object[]
            {
                nameof(MyIdBinderDTO.MyGuid),
                "bf9d993d-1562-49a2-bfef-5b2929447814",
                typeof(DataControllerBase<IEntity<int>, MyIdBinderDTO, MyIdBinderDTO, int>),
                null,
                null,
                null,
                false,
                Guid.Parse("bf9d993d-1562-49a2-bfef-5b2929447814"),
            },
            new object[]
            {
                nameof(MyIdBinderDTO.MyNullableGuid),
                "bf9d993d-1562-49a2-bfef-5b2929447814",
                typeof(DataControllerBase<IEntity<int>, MyIdBinderDTO, MyIdBinderDTO, int>),
                null,
                null,
                null,
                false,
                Guid.Parse("bf9d993d-1562-49a2-bfef-5b2929447814"),
            },
            new object[]
            {
                nameof(MyIdBinderDTO.MyString),
                "Hello World!",
                typeof(DataControllerBase<IEntity<int>, MyIdBinderDTO, MyIdBinderDTO, int>),
                null,
                null,
                null,
                false,
                "Hello World!",
            },
        };

    public static IEnumerable<object[]> BinderShouldNoopTestData => new[]
    {
            new object[]
            {
                nameof(MasterDTO.Id),                                                   // modelName
                "1",                                                                    // modelValueProvided
                typeof(DataControllerBase<IEntity<int>, MasterDTO, MasterDTO, int>),    // baseType
                null,                                                                   // modelType
                null,                                                                   // modelMetadata
                null,                                                                   // modelHashingService
                true,                                                                   // forceProvideNoValue
            }
        };

    private static ControllerContext CreateControllerContext(TypeInfo typeInfo)
    {
        ControllerActionDescriptor actionDescriptor = new() { ControllerTypeInfo = typeInfo };

        return new ControllerContext { ActionDescriptor = actionDescriptor };
    }

    private static TypeInfo CreateTypeInfo(Type baseType) => baseType.GetTypeInfo();

    private static ModelBindingContext CreateModelBindingContext(
        string modelName,
        string modelValueProvided,
        ControllerContext controllerContext,
        Type modelType = default,
        ModelMetadata modelMetadata = default,
        bool forceProvideNoValue = false)
    {
        Mock<ModelBindingContext> modelBindingContext = new(MockBehavior.Strict);
        Mock<IValueProvider> valueProvider = new(MockBehavior.Strict);

        if (string.IsNullOrEmpty(modelName) || forceProvideNoValue)
        {
            valueProvider
                .Setup(p => p.GetValue(It.IsAny<string>()))
                .Returns(ValueProviderResult.None);
        }
        else
        {
            valueProvider
                .Setup(p => p.GetValue(modelName))
                .Returns(new ValueProviderResult(modelValueProvided));
        }

        modelBindingContext.Setup(c => c.ValueProvider).Returns(valueProvider.Object);
        modelBindingContext.Setup(c => c.ActionContext).Returns(controllerContext);

        if (setBinderModelName)
            modelBindingContext.Setup(c => c.BinderModelName).Returns(modelName);
        else
            modelBindingContext.Setup(c => c.BinderModelName).Returns(string.Empty);
        setBinderModelName = !setBinderModelName;

        modelBindingContext.Setup(c => c.ModelName).Returns(modelName);
        modelBindingContext.SetupProperty(c => c.Result);
        modelBindingContext.SetupProperty(x => x.ModelState, new ModelStateDictionary());

        if (modelMetadata is not null)
            modelBindingContext.Setup(c => c.ModelMetadata).Returns(modelMetadata);

        if (modelType is not null)
            modelBindingContext.Setup(c => c.ModelType).Returns(modelType);

        return modelBindingContext.Object;
    }

    private static HashedKeyBinder CreateBinder(IModelHashingService modelHashingService = default)
    {
        Mock<ILoggerFactory> loggerFactory = new(MockBehavior.Strict);
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

        return new(
            loggerFactory.Object,
            modelHashingService ?? Mock.Of<IModelHashingService>(MockBehavior.Strict));
    }

    private static ModelMetadata MetadataForProperty<TContainer>(string propertyName)
    {
        EmptyModelMetadataProvider dataProvider = new();
        return dataProvider.GetMetadataForProperty(typeof(TContainer), propertyName);
    }

    private static ModelMetadata MetadataForType<TModel>()
        => new EmptyModelMetadataProvider().GetMetadataForType(typeof(TModel));

    private static IModelHashingService CreateModelHashingService<TDTO>(
        string propertyName, string hashedPropertyValue, int? unhashedPropertyValue)
    {
        Mock<IModelHashingService> modelHashingService = new();
        modelHashingService
            .Setup(x => x.UnHashProperty(typeof(TDTO), propertyName, hashedPropertyValue))
            .Returns(unhashedPropertyValue);

        return modelHashingService.Object;
    }

    private static void AssertBindingResultNotSet(ModelBindingResult modelBindingResult)
    {
        Assert.False(modelBindingResult.IsModelSet);
        Assert.Null(modelBindingResult.Model);
    }

    private class MyIdBinderDTO : IDTO<int>
    {
        public int Id { get; set; }

        public int? MyNullableInt { get; set; }

        public Guid MyGuid { get; set; }

        public Guid? MyNullableGuid { get; set; }

        public string MyString { get; set; }

        public MasterDTO MyComplexType { get; set; }
    }

    private class IdBinderController : DataControllerBase<IEntity<int>, MyIdBinderDTO, MyIdBinderDTO, int>
    {
        public IdBinderController(IDataService dataService)
            : base(dataService)
        {
        }
    }
}
