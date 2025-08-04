using Microsoft.AspNetCore.Hosting;

using Moq;

using System.Text;

using Xunit;

namespace BBWM.FileStorage.DiskSpace.Test;

public class DiskSpaceStorageProviderTests
{
    private static async Task<(string, string)> SetupFile(
        string filename = "sample.jpeg", string content = "sample content")
    {
        var webRootPath = Path.GetTempPath();
        if (!webRootPath.EndsWith(Path.DirectorySeparatorChar))
            webRootPath += Path.DirectorySeparatorChar;

        var fullPath = Path.Combine(webRootPath, "data", "images", filename);
        var directory = new DirectoryInfo(Path.GetDirectoryName(fullPath));
        if (!directory.Exists)
            directory.Create();

        await File.WriteAllTextAsync(fullPath, content);

        return (webRootPath, fullPath);
    }

    [Fact]
    public async Task Should_Delete_File()
    {
        // Arrange
        var (webRootPath, fullPath) = await SetupFile(filename: "sample-delete.jpeg");

        var webHosting = new Mock<IWebHostEnvironment>();
        webHosting.Setup(h => h.WebRootPath).Returns(webRootPath);
        webHosting.Setup(p => p.EnvironmentName).Returns("test");

        var service = new DiskSpaceStorageProvider(webHosting.Object);

        // Act
        await service.DeleteFile("sample-delete.jpeg", CancellationToken.None);

        // Assert
        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public async Task Should_Get_File_Storage_Data()
    {
        // Arrange
        var (webRootPath, fullPath) = await SetupFile();

        var webHosting = new Mock<IWebHostEnvironment>();
        webHosting.Setup(p => p.WebRootPath).Returns(webRootPath);
        webHosting.Setup(p => p.EnvironmentName).Returns("test");

        var service = new DiskSpaceStorageProvider(webHosting.Object);

        // Act
        var storageData = await service.GetFile("sample.jpeg", CancellationToken.None);

        // Assert
        Assert.NotNull(storageData);
        Assert.Equal("sample.jpeg", storageData.Key);
    }

    [Fact]
    public async Task Should_Not_Get_File_Storage_Date()
    {
        // Arrange
        var (webRootPath, fullPath) = await SetupFile(filename: "not-exists-sample.jpeg");

        var webHosting = new Mock<IWebHostEnvironment>();
        webHosting.Setup(p => p.WebRootPath).Returns(webRootPath);
        webHosting.Setup(p => p.EnvironmentName).Returns("test");

        var service = new DiskSpaceStorageProvider(webHosting.Object);
        File.Delete(fullPath);

        // Act
        var storageData = await service.GetFile("not-exists-sample.jpeg", CancellationToken.None);

        // Assert
        Assert.Null(storageData);
    }

    [Fact]
    public async Task Should_Upload_File()
    {
        // Arrange
        var (webRootPath, fullPath) = await SetupFile(filename: "sample-upload.jpeg");
        File.Delete(fullPath);

        var webHosting = new Mock<IWebHostEnvironment>();
        webHosting.Setup(p => p.WebRootPath).Returns(webRootPath);
        webHosting.Setup(p => p.EnvironmentName).Returns("test");

        var service = new DiskSpaceStorageProvider(webHosting.Object);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample content"));

        // Act
        var storageData = await service.UploadFile(stream, "sample-upload.jpeg", CancellationToken.None);

        // Arrange
        Assert.NotNull(storageData);
        Assert.Equal("sample-upload.jpeg", storageData.Key);
        Assert.True(File.Exists(fullPath));
    }
}
