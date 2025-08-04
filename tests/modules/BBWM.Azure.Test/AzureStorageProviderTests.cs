using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using BBWM.Core.Exceptions;
using BBWM.FileStorage;

using BBWT.Tests.modules.BBWM.Core.Test.Extensions;

using Bogus;

using Microsoft.Extensions.Options;

using Moq;

using System.Text;

using Xunit;

namespace BBWM.Azure.Test;

public class AzureStorageProviderTests
{
    private static readonly AzureSettings ValidAzureSettings =
        new Faker<AzureSettings>()
            .RuleFor(
                p => p.ConnectionString,
                s => "DefaultEndpointsProtocol=https;" +
                     "AccountName=dummyStorageAccountName;" +
                     "AccountKey=DummyKey;" +
                     "BlobEndpoint=http://127.0.0.1:8440/testacc1; " +
                     "FileEndpoint=http://127.0.0.1:8440/testacc1; " +
                     "QueueEndpoint=http://127.0.0.1:8440/testacc1; " +
                     "TableEndpoint=http://127.0.0.1:8440/testacc1")
            .RuleFor(p => p.ContainerName, s => "mycontainer")
            .Generate();

    private const string BlobName = "Azure-XUnit";

    private static readonly Uri BlobUri = new("https://azure.testing.xunit/Azure-XUnit");

    public static IEnumerable<object[]> InvalidAzureSettingsTestData => new[]
    {
            new object[] { null },
            new object[] { ValidAzureSettings.Nullify(s => s.ConnectionString = null) },
            new object[] { ValidAzureSettings.Nullify(s => s.ConnectionString = string.Empty) },
            new object[] { ValidAzureSettings.Nullify(s => s.ContainerName = null) },
            new object[] { ValidAzureSettings.Nullify(s => s.ContainerName = string.Empty) },
        };

    [Theory]
    [MemberData(nameof(InvalidAzureSettingsTestData))]
    public void AzureContainerFactory_Should_Fail_On_Invalid_Settings(AzureSettings azureSettings)
    {
        // Arrange
        var options = new Mock<IOptionsSnapshot<AzureSettings>>();
        options.SetupGet(opts => opts.Value).Returns(azureSettings);

        // Act & Assert
        Assert.Throws<ConflictException>(() => new AzureContainerClientFactory(options.Object));
    }

    [Fact]
    public void AzureContainerFactory_Should_Fail_On_Invalid_Options()
    {
        // Arrange, Act & Assert
        Assert.Throws<ConflictException>(() => new AzureContainerClientFactory(null));
    }

    [Fact]
    public void AzureContainerFactory_Should_Create_Factory()
    {
        // Arrange
        var options = new Mock<IOptionsSnapshot<AzureSettings>>();
        options.SetupGet(opts => opts.Value).Returns(ValidAzureSettings);

        // Act
        var factory = new AzureContainerClientFactory(options.Object);

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public async Task AzureStorageProvider_Should_Upload_File()
    {
        // Arrange
        var (client, azureProvider) = GetService(
            client => client
                .Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Verifiable());
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello!"));

        // Act
        var data = await azureProvider.UploadFile(stream, BlobName, CancellationToken.None);

        // Assert
        AssertStorageData(client, data);
    }

    [Fact]
    public async Task AzureStorageProvider_Should_Get_File()
    {
        // Arrange
        var (client, azureProvider) = GetService(new MyBlobProperties { LastModified = DateTimeOffset.Now });

        // Act
        var data = await azureProvider.GetFile(BlobName, CancellationToken.None);

        // Assert
        AssertStorageData(client, data);
    }

    [Fact]
    public async Task AzureStorageProvider_Should_Get_File_On_Null_Properties()
    {
        // Arrange
        var (client, azureProvider) = GetService((BlobProperties)null);

        // Act
        var data = await azureProvider.GetFile(BlobName, CancellationToken.None);

        // Assert
        AssertStorageData(client, data);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AzureStorageProvider_Should_Delete_File(bool fileExists)
    {
        // Arrange
        var response = Response.FromValue(fileExists, null);

        var (client, azureProvider) = GetService(
            client => client
                .Setup(c => c.DeleteIfExistsAsync(
                    It.IsAny<DeleteSnapshotsOption>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response)
                .Verifiable());

        // Act
        var exists = await azureProvider.DeleteFile(BlobName, CancellationToken.None);

        // Assert
        Assert.Equal(fileExists, exists);
        client.Verify();
    }

    private static (Mock<BlobClient>, AzureStorageProvider) GetService(BlobProperties properties)
    {
        var response = Response.FromValue(properties, null);

        return GetService(
            client => client
                .Setup(c => c.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response));
    }

    private static (Mock<BlobClient>, AzureStorageProvider) GetService(Action<Mock<BlobClient>> setupClient)
    {
        var mockAzureSettingProvider = new Mock<IOptionsSnapshot<AzureSettings>>();
        mockAzureSettingProvider.SetupGet(p => p.Value).Returns(ValidAzureSettings);

        var blobClient = new Mock<BlobClient>();
        blobClient.SetupGet(c => c.Uri).Returns(BlobUri);
        setupClient?.Invoke(blobClient);

        var blobContainerClient = new Mock<BlobContainerClient>();
        blobContainerClient.Setup(cc => cc.GetBlobClient(It.IsAny<string>())).Returns(blobClient.Object);

        var containerClientFactory = new Mock<IAzureContainerClientFactory>();
        containerClientFactory
            .Setup(f => f.CreateContainerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobContainerClient.Object);

        return (blobClient, new AzureStorageProvider(containerClientFactory.Object));
    }

    private static void AssertStorageData(Mock<BlobClient> client, StorageFileData data)
    {
        Assert.NotNull(data);
        Assert.Equal(BlobUri, new(data.Url));
        Assert.Equal(BlobName, data.Key);

        client.Verify();
    }

    private class MyBlobProperties : BlobProperties
    {
        public new DateTimeOffset LastModified { get; set; }
    }
}
