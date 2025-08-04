using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

using BBWM.Core.Exceptions;

using Bogus;

using Microsoft.Extensions.Options;

using Moq;

using System.Net;
using System.Text;

using Xunit;

namespace BBWM.AWS.Test;

public class AwsServiceTest
{
    public AwsServiceTest()
    {
    }

    private static IAmazonS3 S3Client
    {
        get
        {
            var s3Client = new Mock<IAmazonS3>();

            var presignedUrl = $"https://fake-s3.bucket/?token={Guid.NewGuid():N}";
            s3Client.Setup(c => c.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns(presignedUrl);

            s3Client
                .Setup(c => c.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.NoContent });

            s3Client
                .Setup(c => c.ListObjectsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListObjectsResponse { S3Objects = new List<S3Object>() });

            var response = new MemoryStream(Encoding.UTF8.GetBytes("Hi There!"));
            s3Client
                .Setup(c => c.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse { ResponseStream = response });

            return s3Client.Object;
        }
    }

    private static AwsService GetService()
    {
        var awsSettings = new Faker<AwsSettings>()
           .RuleFor(p => p.AccessKeyId, s => s.Random.AlphaNumeric(7))
           .RuleFor(p => p.SecretAccessKey, s => s.Random.AlphaNumeric(7))
           .RuleFor(p => p.ParametersPath, s => s.Random.AlphaNumeric(7))
           .RuleFor(p => p.ParametersReloadingInterval, s => s.Random.Int())
           .RuleFor(p => p.AwsRegion, s => s.Random.AlphaNumeric(7))
           .RuleFor(p => p.BucketName, s => s.Random.AlphaNumeric(7))
           .RuleFor(p => p.S3Url, s => s.Random.AlphaNumeric(7))
           .Generate();

        var mock = new Mock<IOptionsSnapshot<AwsSettings>>();
        mock.Setup(p => p.Value).Returns(awsSettings);

        return new AwsService(mock.Object, S3Client, Mock.Of<ITransferUtility>());
    }

    private static AwsService GetServiceFailed()
    {
        var awsSettingsMock = new AwsSettings
        {
            BucketName = null,
            AccessKeyId = null,
            AwsRegion = null,
            SecretAccessKey = null,
        };

        var mock = new Mock<IOptionsSnapshot<AwsSettings>>();
        mock.Setup(p => p.Value).Returns(awsSettingsMock);

        return new AwsService(mock.Object, S3Client, Mock.Of<ITransferUtility>());
    }

    [Fact]
    public async Task Check_Permissions_Test()
    {
        var service = GetService();
        var result = await service.CheckPermissions();
        var result2 = service.GetRegions();

        var serviceFailed = GetServiceFailed();
        var resultFailed = serviceFailed.CheckPermissions();

        Assert.NotNull(result);
        Assert.NotNull(result2);
        Assert.NotNull(resultFailed);
    }

    [Fact]
    public void Generate_PreSigned_URL_Test()
    {
        var service = GetService();
        var result = service.GeneratePreSignedURL("test");

        var serviceFailed = GetServiceFailed();
        Action resultFailed = () => serviceFailed.GeneratePreSignedURL("test");

        Assert.NotNull(result);
        Assert.NotNull(resultFailed);
        Assert.Throws<ConflictException>(resultFailed);
    }

    [Fact]
    public async Task Delete_File_Test()
    {
        var service = GetService();
        var result = await service.DeleteFile("test");

        var serviceFailed = GetServiceFailed();
        var resultFailed = await serviceFailed.DeleteFile("test");

        Assert.True(result);
        Assert.False(resultFailed);
    }

    [Fact]
    public async Task Get_All_Files_Test()
    {
        var service = GetService();
        var result = await service.GetAllFiles(CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_All_Images_Test()
    {
        var service = GetService();
        var result = await service.GetAllImages(CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Upload_File_Test()
    {
        var service = GetService();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
        var result = await service.UploadFile(stream, "test_key", CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Get_File_Test()
    {
        var service = GetService();
        var result = await service.GetFile("test", CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Download_File_Test()
    {
        var service = GetService();
        var result = await service.DownloadFile("test", CancellationToken.None);
        Assert.NotNull(result);
    }
}
