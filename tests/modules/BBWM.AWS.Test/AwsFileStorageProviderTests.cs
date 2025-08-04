
using Moq;

using System.Text;

using Xunit;

namespace BBWM.AWS.Test;

public class AwsFileStorageProviderTests
{
    private const string Aws_Key = "DUMMY AWS KEY";
    private readonly CancellationToken ct = CancellationToken.None;

    [Fact]
    public async Task Should_Upload_File()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello AWS!"));
        var (awsService, service) = CreateService(
            awsService => awsService.Setup(s => s.UploadFile(stream, Aws_Key, ct)).Verifiable());

        // Act
        await service.UploadFile(stream, Aws_Key, ct);

        // Assert
        awsService.Verify();
    }

    [Fact]
    public async Task Should_Get_File()
    {
        // Arrange
        var (awsService, service) = CreateService(
            awsService => awsService.Setup(s => s.GetFile(Aws_Key, ct)).Verifiable());

        // Act
        await service.GetFile(Aws_Key, ct);

        // Assert
        awsService.Verify();
    }

    [Fact]
    public async Task Should_Delete_File()
    {
        // Arrange
        var (awsService, service) = CreateService(
            awsService => awsService.Setup(s => s.DeleteFile(Aws_Key)).Verifiable());

        // Act
        await service.DeleteFile(Aws_Key, ct);

        // Assert
        awsService.Verify();
    }

    [Fact]
    public async Task Should_Download_File()
    {
        // Arrange
        var (awsService, service) = CreateService(
            awsService => awsService.Setup(s => s.DownloadFile(Aws_Key, ct)).Verifiable());

        // Act
        await service.DownloadFile(Aws_Key, ct);

        // Assert
        awsService.Verify();
    }

    private static (Mock<IAwsService>, AwsFileStorageProvider) CreateService(
        Action<Mock<IAwsService>> setupAwsService = default)
    {
        var awsService = new Mock<IAwsService>();
        setupAwsService?.Invoke(awsService);

        return (awsService, new AwsFileStorageProvider(awsService.Object));
    }
}
