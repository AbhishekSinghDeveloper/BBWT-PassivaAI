using AutoMapper;

using BBWM.Core.DTO;
using BBWM.Core.Test.Fixtures;
using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;
using BBWM.DataProcessing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

using Moq;

using Xunit;

namespace BBWT.Tests.modules.BBWM.DataProcessing.Test.Services;

public class BaseImportServiceTests : IClassFixture<MappingFixture>
{
    private const string DefaultFilename = "default.png";
    private static readonly string DateFormats = "yyyy-MM-dd";

    public BaseImportServiceTests(MappingFixture mappingFixture)
    {
        Mapper = mappingFixture.DefaultMapper;
    }

    public IMapper Mapper { get; }

    [Fact]
    public async Task Import_Operation_Should_Stop()
    {
        // Arrange
        var (importService, clientProxy) = CreateImportService(
            clientProxy => clientProxy
                .Setup(p => p.SendCoreAsync("Stopped", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable());
        CancellationTokenSource tokenSource = new(TimeSpan.Zero);

        // Act
        await importService.Import(new(), tokenSource.Token);

        // Assert
        clientProxy.Verify();
    }

    [Fact]
    public async Task Import()
    {
        // Arrange
        var (importService, clientProxy) = CreateImportService(
            clientProxy =>
            {
                clientProxy
                    .Setup(p => p.SendCoreAsync("Update", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
                clientProxy
                    .Setup(p => p.SendCoreAsync("Result", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
            });

        // Act
        DataImportResultDTO result = await importService.Import(new(), CancellationToken.None);

        // Assert
        Assert.Equal(1, result?.ImportedCount ?? 0);
        clientProxy.Verify();
    }

    [Fact]
    public async Task Import_Should_Import_Nothing()
    {
        // Arrange
        ImportEntry importEntry = CreateEntry(new() { Id = 1, Name = "John Doe" }, true);

        var (importService, clientProxy) = CreateImportService(
            clientProxy => clientProxy
                .Setup(p => p.SendCoreAsync("Result", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable(),
            importEntry);

        // Act
        DataImportResultDTO result = await importService.Import(new(), CancellationToken.None);

        // Assert
        Assert.Equal(0, result?.ImportedCount ?? -1);
        clientProxy.Verify();
    }

    [Theory]
    [MemberData(nameof(CreateSettingsShouldThrowTestData))]
    public void CreateSettings_Should_Throw_On_Invalid_Column(ImportDataModel importDataModel, Type exceptionType)
    {
        // Arrange
        TestBaseImportService importService = CreateImportServiceBasic();

        // Act & Assert
        Exception exception = Assert.ThrowsAny<Exception>(() => importService.CreateSettings(importDataModel, new MemoryStream(), "1"));
        Assert.IsType(exceptionType, exception);
    }

    [Theory]
    [MemberData(nameof(CreateSettingsTestData))]
    public void CreateSettings(ImportDataModel importDataModel, string expectedFilename)
    {
        // Arrange
        TestBaseImportService importService = CreateImportServiceBasic();

        // Act
        DataImportConfig config = importService.CreateSettings(importDataModel, new MemoryStream(), "1");

        // Assert
        Assert.Equal("1", config?.UserId);
        Assert.Equal(1, config?.ColumnDefinitions?.Count ?? 0);
        Assert.Equal(expectedFilename, config?.FileName);
    }

    [Theory]
    [MemberData(nameof(CreateSettingsNoOverrideShouldThrowTestData))]
    public void CreateSettings_No_Override_Should_Throw_On_Invalid_Column(ImportDataModel importDataModel, Type exceptionType)
    {
        // Arrange
        TestNoOverrideBaseImportService importService = new(Mock.Of<IDataImportHelper>(), Mock.Of<IHubContext<DataImportHub>>(), Mapper);

        // Act & Assert
        Exception exception = Assert.ThrowsAny<Exception>(() => importService.CreateSettings(importDataModel, new MemoryStream(), "1"));
        Assert.IsType(exceptionType, exception);
    }

    public static IEnumerable<object[]> CreateSettingsTestData => new[]
    {
            new object[]
            {
                new ImportDataModel
                {
                    Config = new() { ColumnDefinitions = new[] { new ColumnDefinitionDTO { OrderNumber = 1, TargetFieldName = "Id" } } },
                    File = CreateFormFile("test.png"),
                },
                "test.png",
            },
            new object[]
            {
                new ImportDataModel
                {
                    Config = new()
                    {
                        ColumnDefinitions = new[]
                        {
                            new ColumnDefinitionDTO
                            {
                                OrderNumber = 1,
                                TargetFieldName = "Id",
                                Type = CellDataType.Number,
                                TypeInfo = new CellDataTypeInfoDTO { Min = 1, Max = 1000 },
                            },
                        },
                    },
                    File = CreateFormFile("test.png"),
                },
                "test.png",
            },
            new object[]
            {
                new ImportDataModel
                {
                    Config = new()
                    {
                        ColumnDefinitions = new[]
                        {
                            new ColumnDefinitionDTO
                            {
                                OrderNumber = 1,
                                TargetFieldName = "Id",
                                Type = CellDataType.Date,
                                TypeInfo = new CellDataTypeInfoDTO { DateFormats = DateFormats },
                            },
                        },
                    },
                    File = CreateFormFile("test.png"),
                },
                "test.png",
            },
            new object[]
            {
                new ImportDataModel
                {
                    Config = new()
                    {
                        ColumnDefinitions = new[]
                        {
                            new ColumnDefinitionDTO
                            {
                                OrderNumber = 1,
                                TargetFieldName = "Id",
                                Type = CellDataType.DateTimeOffset,
                                TypeInfo = new CellDataTypeInfoDTO { DateFormats = DateFormats },
                            },
                        },
                    },
                    File = CreateFormFile("test.png"),
                },
                "test.png",
            },
            new object[] { new ImportDataModel() { File = CreateFormFile(DefaultFilename) }, DefaultFilename },
        };

    public static IEnumerable<object[]> CreateSettingsShouldThrowTestData => new[]
    {
            new object[] { null, typeof(ArgumentNullException) },
            new object[]
            {
                new ImportDataModel()
                {
                    Config = new()
                    {
                        ColumnDefinitions = new[] { (ColumnDefinitionDTO)null },
                    },
                },
                typeof(NullReferenceException),
            },
            new object[]
            {
                new ImportDataModel()
                {
                    Config = new()
                    {
                        ColumnDefinitions = new[] { new ColumnDefinitionDTO { OrderNumber = -1 } },
                    },
                },
                typeof(ArgumentException),
            },
        };

    public static IEnumerable<object[]> CreateSettingsNoOverrideShouldThrowTestData => new[]
    {
            new object[]
            {
                new ImportDataModel()
                {
                    Config = new()
                    {
                        ColumnDefinitions = new[] { new ColumnDefinitionDTO { OrderNumber = -1, Type = CellDataType.Custom } },
                    },
                },
                typeof(NullReferenceException),
            },
            new object[] { new ImportDataModel(), typeof(NullReferenceException) },
        };

    private TestBaseImportService CreateImportServiceBasic()
        => new(Mock.Of<IDataImportHelper>(), Mock.Of<IHubContext<DataImportHub>>(), Mapper);

    private static IFormFile CreateFormFile(string filename)
    {
        Mock<IFormFile> formFile = new();
        formFile.Setup(f => f.FileName).Returns(filename);

        return formFile.Object;
    }

    private (TestBaseImportService, Mock<IClientProxy>) CreateImportService(
        Action<Mock<IClientProxy>> setupProxy = default, ImportEntry importEntry = default)
    {
        Mock<IDataImportHelper> dataImportHelper = new();
        dataImportHelper
            .Setup(h => h.ProcessDataImport(It.IsAny<DataImportConfig>(), It.IsAny<OnEntryProcessedCallback>()))
            .Returns((DataImportConfig config, OnEntryProcessedCallback callback) =>
            {
                importEntry ??= CreateEntry(new() { Id = 1, Name = "John Doe" });
                callback?.Invoke(new(importEntry));
                return new(new[] { importEntry });
            });

        Mock<IClientProxy> clientProxy = new();
        setupProxy?.Invoke(clientProxy);

        Mock<IHubClients> clients = new();
        clients.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxy.Object);

        Mock<IHubContext<DataImportHub>> hubContext = new();
        hubContext.Setup(hub => hub.Clients).Returns(clients.Object);

        TestBaseImportService importService = new(dataImportHelper.Object, hubContext.Object, Mapper);

        return (importService, clientProxy);
    }

    private static ImportEntry CreateEntry(TestImportDTO originalData, bool shouldBeInvalid = false)
        => new(new object[] { originalData })
        {
            Cells = new List<ImportEntryCell>
            {
                    new ImportEntryCell(
                        originalData.Id,
                        new ColumnDefinition
                        {
                            TargetFieldName = nameof(TestImportDTO.Id),
                            Position = 1,
                        })
                    {
                        ErrorMessage = shouldBeInvalid ? "Invalid value" : null,
                    },
                    new ImportEntryCell(
                        originalData.Name,
                        new ColumnDefinition
                        {
                            TargetFieldName = nameof(TestImportDTO.Name),
                            Position = 2,
                        })
                    {
                        ErrorMessage = shouldBeInvalid ? "Invalid value" : null,
                    },
                    new ImportEntryCell(
                        originalData.Age,
                        new ColumnDefinition
                        {
                            TargetFieldName = nameof(TestImportDTO.Age),
                            Position = 3,
                        })
                    {
                        ErrorMessage = shouldBeInvalid ? "Invalid value" : null,
                    },
            },
            ErrorMessage = shouldBeInvalid ? "Invalid Entry" : null,
        };

    private class TestImportDTO : IDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Age { get; set; }
    }

    private class TestBaseImportService : BaseImportService<TestImportDTO>
    {
        public TestBaseImportService(
            IDataImportHelper dataImportHelper,
            IHubContext<DataImportHub> hubContext,
            IMapper mapper)
            : base(dataImportHelper, hubContext, mapper)
        {
        }

        protected override Task OnEntityImport(TestImportDTO entity, CancellationToken ct) => Task.CompletedTask;

        protected override Task SaveImportedEntities(IEnumerable<TestImportDTO> list, CancellationToken ct) => Task.CompletedTask;

        protected override CustomValidationHandler GetCustomValidator(CellDataTypeInfoDTO typeInfo) => entryCell => { };

        protected override DataImportConfigDTO GetDefaultConfig(ImportDataModel importDataModel)
            => new()
            {
                ColumnDefinitions = new[]
                {
                        new ColumnDefinitionDTO
                        {
                            OrderNumber = 1,
                            TargetFieldName = "Id",
                            Type = CellDataType.Custom,
                        },
                },
                FileName = importDataModel.FileName,
            };
    }

    private class TestNoOverrideBaseImportService : BaseImportService<TestImportDTO>
    {
        public TestNoOverrideBaseImportService(
            IDataImportHelper dataImportHelper,
            IHubContext<DataImportHub> hubContext,
            IMapper mapper)
            : base(dataImportHelper, hubContext, mapper)
        {
        }

        protected override Task OnEntityImport(TestImportDTO entity, CancellationToken ct) => Task.CompletedTask;

        protected override Task SaveImportedEntities(IEnumerable<TestImportDTO> list, CancellationToken ct) => Task.CompletedTask;
    }
}
