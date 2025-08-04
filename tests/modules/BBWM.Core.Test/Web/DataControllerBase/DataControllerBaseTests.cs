using BBWM.Core.Data;
using BBWM.Core.DTO;
using BBWM.Core.Filters;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;

using Bogus;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;

using Xunit;

namespace BBWM.Core.Test.Web.DataControllerBase;

public class DataControllerBaseTests
{
    private const int DefaultId = 1;
    private const string IdHashSuffix = "-UT123";


    [Fact]
    public async Task Get_Should_Get_From_DataHandler()
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataHandler = new Mock<IEntityGet<MyDTO, int>>();
        dataHandler
            .Setup(h => h.Get(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDTO)
            .Verifiable();

        MyController dataController = new(DefaultDataService, dataHandler.Object);

        // Act
        var result = await dataController.Get(DefaultId, CancellationToken.None);

        // Assert
        dataHandler.Verify();
        AssertOkResult<MyDTO>(result, dto => Assert.Equal(myDTO, dto, myDTO));
    }

    [Fact]
    public async Task Get_Should_Get_From_DataService()
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataService = new Mock<IDataService>();
        dataService
            .Setup(ds => ds.Get<MyEntity, MyDTO, int>(
                It.IsAny<int>(),
                It.IsAny<IEntityQuery<MyEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDTO)
            .Verifiable();

        MyController dataController = new(dataService.Object, Mock.Of<IDataHandler>());

        // Act
        var result = await dataController.Get(DefaultId, CancellationToken.None);

        // Assert
        dataService.Verify();
        AssertOkResult<MyDTO>(result, dto => Assert.Equal(myDTO, dto, myDTO));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_Should_Create_From_DataHandler(bool shouldHashId)
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataHandler = new Mock<IEntityCreate<MyDTO>>();
        dataHandler
            .Setup(h => h.Create(It.IsAny<MyDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDTO)
            .Verifiable();
        MyController dataController = new(DefaultDataService, dataHandler.Object);
        var modelHashingService = SetupIdHasher(shouldHashId, DefaultId);

        // Act
        var result = await dataController.Create(myDTO, modelHashingService.Object, CancellationToken.None);

        // Assert
        dataHandler.Verify();
        modelHashingService.Verify();

        AssertCreatedResult<MyDTO>(
            result,
            shouldHashId ? GetIdHash(DefaultId) : $"{DefaultId}",
            dto => Assert.Equal(myDTO, dto, myDTO));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_Should_Create_From_DataService(bool shouldHashId)
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataService = new Mock<IDataService>();
        dataService
            .Setup(ds => ds.Create<MyEntity, MyDTO>(It.IsAny<MyDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDTO)
            .Verifiable();
        MyController dataController = new(dataService.Object, DefaultDataHandler);
        var modelHashingService = SetupIdHasher(shouldHashId, DefaultId);

        // Act
        var result = await dataController.Create(myDTO, modelHashingService.Object, CancellationToken.None);

        // Assert
        dataService.Verify();
        modelHashingService.Verify();

        AssertCreatedResult<MyDTO>(
            result,
            shouldHashId ? GetIdHash(DefaultId) : $"{DefaultId}",
            dto => Assert.Equal(myDTO, dto, myDTO));
    }

    [Fact]
    public async Task Update_Should_Update_From_DataHandler()
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataHandler = new Mock<IEntityUpdate<MyDTO>>();
        dataHandler
            .Setup(h => h.Update(It.IsAny<MyDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDTO)
            .Verifiable();
        MyController dataController = new(DefaultDataService, dataHandler.Object);

        // Act
        var result = await dataController.Update(myDTO, CancellationToken.None);

        // Assert
        dataHandler.Verify();
        AssertOkResult<MyDTO>(result, dto => Assert.Equal(myDTO, dto, myDTO));
    }

    [Fact]
    public async Task Update_Should_Update_From_DataService()
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataService = new Mock<IDataService>();
        dataService
            .Setup(ds => ds.Update<MyEntity, MyDTO, int>(It.IsAny<MyDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDTO)
            .Verifiable();
        MyController dataController = new(dataService.Object, DefaultDataHandler);

        // Act
        var result = await dataController.Update(myDTO, CancellationToken.None);

        // Assert
        dataService.Verify();
        AssertOkResult<MyDTO>(result, dto => Assert.Equal(myDTO, dto, myDTO));
    }

    [Fact]
    public async Task Delete_Should_Delete_FromDataHandler()
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataHandler = new Mock<IEntityDelete<int>>();
        dataHandler
            .Setup(h => h.Delete(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        MyController dataController = new(DefaultDataService, dataHandler.Object);

        // Act
        var result = await dataController.Delete(DefaultId, CancellationToken.None);

        // Assert
        dataHandler.Verify();
        AssertOkResult<int>(result, dtoId => Assert.Equal(DefaultId, dtoId));
    }

    [Fact]
    public async Task Delete_Should_Delete_FromDataService()
    {
        // Arrange
        MyDTO myDTO = DefaultDTO;
        var dataService = new Mock<IDataService>();
        dataService
            .Setup(ds => ds.Delete<MyEntity, int>(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        MyController dataController = new(dataService.Object, DefaultDataHandler);

        // Act
        var result = await dataController.Delete(DefaultId, CancellationToken.None);

        // Assert
        dataService.Verify();
        AssertOkResult<int>(result, dtoId => Assert.Equal(DefaultId, dtoId));
    }

    [Fact]
    public async Task GetPage_Should_GetPage_From_DataHandler()
    {
        // Arrange
        List<MyDTO> myDTOs = FakeDTO(DefaultId).Generate(20).OrderBy(dto => dto.Id).ToList();
        PageResult<MyDTO> pageResult = new()
        {
            Items = myDTOs,
            Total = myDTOs.Count,
        };
        var dataHandler = new Mock<IEntityPage<MyDTO>>();
        dataHandler
            .Setup(h => h.GetPage(It.IsAny<QueryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);
        MyController dataController = new(DefaultDataService, dataHandler.Object);

        // Act
        var result = await dataController.GetPage(new(), CancellationToken.None);

        // Assert
        AssertOkResult(result, AssertPageResult(pageResult));
    }

    [Fact]
    public async Task GetPage_Should_GetPage_From_DataService()
    {
        // Arrange
        List<MyDTO> myDTOs = FakeDTO(DefaultId).Generate(20).OrderBy(dto => dto.Id).ToList();
        PageResult<MyDTO> pageResult = new()
        {
            Items = myDTOs,
            Total = myDTOs.Count,
        };
        var dataService = new Mock<IDataService>();
        dataService
            .Setup(h => h.GetPage<MyEntity, MyDTO>(
                It.IsAny<QueryCommand>(),
                It.IsAny<IEntityQuery<MyEntity>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(pageResult);
        MyController dataController = new(dataService.Object, DefaultDataHandler);

        // Act
        var result = await dataController.GetPage(new(), CancellationToken.None);

        // Assert
        AssertOkResult(result, AssertPageResult(pageResult));
    }

    private static IDataService DefaultDataService => Mock.Of<IDataService>();

    private static IDataHandler DefaultDataHandler => Mock.Of<IDataHandler>();

    private static Mock<IModelHashingService> SetupIdHasher(bool shouldHashId, int id)
    {
        var modelHashingService = new Mock<IModelHashingService>();

        if (shouldHashId)
        {
            modelHashingService
                .Setup(s => s.HashProperty(typeof(MyDTO), nameof(MyDTO.Id), id))
                .Returns((Type t, string name, int dtoId) => GetIdHash(dtoId))
                .Verifiable();
        }

        return modelHashingService;
    }

    private static MyDTO DefaultDTO => FakeDTO().Generate();

    private static Faker<MyDTO> FakeDTO(int startId = DefaultId)
    {
        int id = startId;
        return new Faker<MyDTO>()
            .RuleFor(dto => dto.Id, _ => id++)
            .RuleFor(dto => dto.Name, f => f.Random.AlphaNumeric(10));
    }

    private static string GetIdHash(int id) => $"{id}{IdHashSuffix}";

    private static void AssertOkResult<TDTO>(IActionResult actionResult, Action<TDTO> asserts = default)
    {
        var result = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        var wrappedDTO = Assert.IsType<TDTO>(result.Value);
        asserts?.Invoke(wrappedDTO);
    }

    private static void AssertCreatedResult<TDTO>(IActionResult actionResult, string expectedHashId, Action<TDTO> asserts)
    {
        var result = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        Assert.EndsWith(expectedHashId, result.Location);
        var wrappedDTO = Assert.IsType<TDTO>(result.Value);
        asserts?.Invoke(wrappedDTO);
    }

    private static Action<PageResult<MyDTO>> AssertPageResult(PageResult<MyDTO> expectedPage)
    {
        return actualPage =>
        {
            Assert.Equal(expectedPage.Total, actualPage.Total);
            var zipped = expectedPage.Items.Zip(
                actualPage.Items.OrderBy(dto => dto.Id),
                (expected, actual) => new { Expected = expected, Actual = actual });
            Assert.All(zipped, z => Assert.Equal(z.Expected, z.Actual, z.Expected));
        };
    }
}

public class MyController : DataControllerBase<MyEntity, MyDTO, MyDTO>
{
    public MyController(
        IDataService<IDbContext> dataService,
        IDataHandler dataHandler)
        : base(dataService, dataHandler)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };
    }
}

public class MyEntity : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class MyDTO : IDTO, IEqualityComparer<MyDTO>
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool Equals(MyDTO x, MyDTO y)
        => x.Id == y.Id && string.Compare(x.Name, y.Name, false, CultureInfo.InvariantCulture) == 0;

    public int GetHashCode([DisallowNull] MyDTO obj) => Id.GetHashCode();
}
