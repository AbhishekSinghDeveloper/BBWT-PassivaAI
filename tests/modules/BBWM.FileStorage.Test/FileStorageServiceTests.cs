using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Test.Utils;

using Bogus;

using Moq;

using Xunit;

namespace BBWM.FileStorage.Test;

public class FileStorageServiceTests : IClassFixture<FileStorageFixture>
{
    private IMapper _mapper;
    private Mock<IFileStorageProvider> storageProvider;

    public FileStorageServiceTests(FileStorageFixture storageFixture)
    {
        _mapper = storageFixture.Mapper;
        storageProvider = storageFixture.FileStorageProvider;
    }

    private List<FileDetails> CreateFiles(int count)
    {
        var id = 1;
        return new Faker<FileDetails>()
            .RuleFor(f => f.Id, _ => id++)
            .RuleFor(f => f.FileName, faker => faker.Random.String(5, 10, 'a', 'z'))
            .RuleFor(f => f.OperationName, faker => faker.Random.String(3, 8, 'a', 'z'))
            .RuleFor(f => f.Extension, faker => faker.Random.String(3, 5, 'a', 'z'))
            .RuleFor(f => f.IsImage, faker => faker.Random.Bool())
            .RuleFor(f => f.UserId, faker => faker.Random.String(1, 3, '2', '9'))
            .Generate(count);
    }

    private async Task<(FileStorageService, List<FileDetails>)> GetService(int count = 1, IDbContext dbContext = null)
    {
        var fileDetails = CreateFiles(count);

        if (dbContext is null)
            dbContext = SutDataHelper.CreateEmptyContext<IDbContext>();
        await SutDataHelper.InsertData(dbContext, fileDetails.ToArray());

        return (
            new FileStorageService(_mapper, dbContext, storageProvider.Object),
            fileDetails);
    }

    [Fact]
    public async Task Should_Get_File()
    {
        // Arrange
        var (service, files) = await GetService();

        // Act
        var file = await service.Get(1);

        // Assert
        AssertFile(files[0], file);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Should_Get_All_Files(int filesCount)
    {
        // Arrange
        var (service, files) = await GetService(filesCount);

        // Act
        var allFiles = await service.GetAllFiles(CancellationToken.None);

        // Assert
        Assert.NotEmpty(allFiles);
        Assert.Equal(filesCount, allFiles.Count);
        Assert.All(allFiles, actual => AssertFile(files.Find(f => f.Id == actual.Id), actual));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Should_Get_All_Files_With_Operation(int filesCount)
    {
        // Arrange
        int id = filesCount + 1;
        var initialFiles = CreateFiles(3);
        initialFiles.ForEach(f =>
        {
            f.Id = id++;
            f.OperationName = "OperationName";
        });
        var dbContext = await SutDataHelper
            .CreateContextWithData<IDbContext, FileDetails>(initialFiles.ToArray());

        var (service, _) = await GetService(filesCount, dbContext);

        // Act
        var allFiles = await service.GetAllFiles("OperationName", CancellationToken.None);

        // Assert
        Assert.NotEmpty(allFiles);
        Assert.Equal(3, allFiles.Count);
        Assert.All(allFiles, actual => AssertFile(initialFiles.Find(f => f.Id == actual.Id), actual));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Should_Get_All_Images(int filesCount)
    {
        // Arrange
        var (service, files) = await GetService(filesCount);
        var images = files.Where(f => f.IsImage).ToList();

        // Act
        var allImages = await service.GetAllImages(CancellationToken.None);

        // Assert
        Assert.Equal(images.Count, allImages.Count);
        Assert.All(allImages, actual => AssertFile(images.Find(f => f.Id == actual.Id), actual));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Should_Get_All_Images_With_Operation(int filesCount)
    {
        // Arrange
        int id = filesCount + 1;
        var initialFiles = CreateFiles(5);
        initialFiles.ForEach(f =>
        {
            f.Id = id++;
            f.OperationName = "OperationName";
        });
        var dbContext = await SutDataHelper
            .CreateContextWithData<IDbContext, FileDetails>(initialFiles.ToArray());

        var (service, _) = await GetService(filesCount, dbContext);
        var images = initialFiles.Where(f => f.IsImage).ToList();

        // Act
        var allFiles = await service.GetAllImages("OperationName", CancellationToken.None);

        // Assert
        Assert.Equal(images.Count, allFiles.Count);
        Assert.All(allFiles, actual => AssertFile(images.Find(f => f.Id == actual.Id), actual));
    }

    [Fact]
    public async Task Complete_Users_Files_Uploading_Operation_Test()
    {
        // Arrange
        var initialFiles = CreateFiles(5);
        var id = 6;
        initialFiles.ForEach(f =>
        {
            f.Id = id++;
            f.OperationName = "OperationName";
            f.UserId = "1";
        });
        var dbContext = await SutDataHelper
            .CreateContextWithData<IDbContext, FileDetails>(initialFiles.ToArray());

        var (service, files) = await GetService(5, dbContext);

        // Act
        await service.CompleteUsersFilesUploadingOperation("1", "OperationName", CancellationToken.None);

        // Assert
        Assert.All(files, f =>
        {
            Assert.NotNull(f.UserId);
            Assert.NotNull(f.OperationName);
        });
        Assert.All(initialFiles, f =>
        {
            Assert.Null(f.UserId);
            Assert.Null(f.OperationName);
        });
    }

    private static void AssertFile(FileDetails expected, FileDetailsDTO actual)
    {
        Assert.NotNull(actual);
        Assert.Equal($"{expected.FileName}.{expected.Extension}", actual.FileName);
    }
}
