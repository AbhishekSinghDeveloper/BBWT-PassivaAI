using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

using BBWM.Core.Exceptions;
using BBWM.FileStorage;

using Microsoft.Extensions.Options;

using System.Net;

namespace BBWM.AWS;

public class AwsService : IAwsService
{
    private readonly IOptionsSnapshot<AwsSettings> _s3Settings;
    private readonly IAmazonS3 _s3Client;
    private readonly ITransferUtility _transferUtility;

    private static Exception AwsNotSpecifiedException =>
        new ConflictException("Your AWS credentials are not specified. Please specify your AWS credentials on System Configuration page.");


    public AwsService(IOptionsSnapshot<AwsSettings> s3Settings, IAmazonS3 s3Client, ITransferUtility transferUtility)
    {
        _s3Settings = s3Settings;
        _s3Client = s3Client;
        _transferUtility = transferUtility;
    }

    public ICollection<RegionEndpoint> GetRegions() => RegionEndpoint.EnumerableAllRegions.ToList();

    public async Task<AwsCheckPermissionsResult> CheckPermissions()
    {
        try
        {
            var s3Settings = _s3Settings.Value;
            var keyName = "test_file";
            var testFolder = "test_folder/";

            if (string.IsNullOrEmpty(s3Settings.AwsRegion))
            {
                return new AwsCheckPermissionsResult("Please specify AWS region!", false);
            }

            // try to create a test folder
            var request = new PutObjectRequest
            {
                BucketName = s3Settings.BucketName,
                Key = testFolder
            };

            await _s3Client.PutObjectAsync(request).ConfigureAwait(false);

            using (var stream = new MemoryStream())
            {
                using var writer = new StreamWriter(stream);
                writer.WriteLine("Hello");
                writer.WriteLine("And");
                writer.WriteLine("Welcome");
                writer.Flush();
                stream.Position = 0;

                request = new PutObjectRequest
                {
                    BucketName = s3Settings.BucketName,
                    Key = testFolder + keyName,
                    //FilePath = path,
                    InputStream = stream
                };
                await _s3Client.PutObjectAsync(request).ConfigureAwait(false);
            }

            // read the object data and its metadata
            var obj_request = new ListObjectsRequest
            {
                BucketName = s3Settings.BucketName,
                Prefix = testFolder + keyName
            };
            await _s3Client.ListObjectsAsync(obj_request).ConfigureAwait(false);

            // try to delete the test object and test folder from the default bucket
            await _s3Client.DeleteObjectAsync(s3Settings.BucketName, testFolder + keyName).ConfigureAwait(false);
            await _s3Client.DeleteObjectAsync(s3Settings.BucketName, testFolder).ConfigureAwait(false);

            return new AwsCheckPermissionsResult("Your AWS Credentials have been checked successfully.");
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.ErrorCode is not null && (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity")))
            {
                return new AwsCheckPermissionsResult("Please check the provided AWS Credentials.", false);
            }

            throw;
        }
    }

    public string GeneratePreSignedURL(string key)
    {
        try
        {
            var s3Settings = _s3Settings.Value;
            if (SettingsIsValid(s3Settings))
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = s3Settings.BucketName,
                    Key = key,
                    Expires = DateTime.Now.AddHours(3)
                };

                return _s3Client.GetPreSignedURL(request);
            }
            throw AwsNotSpecifiedException;
        }
        catch (AmazonS3Exception ex)
        {
            throw HandleS3Exception(ex);
        }
    }

    public async Task<bool> DeleteFile(string key)
    {
        try
        {
            var s3Settings = _s3Settings.Value;

            if (SettingsIsValid(s3Settings))
            {
                var response = await _s3Client.DeleteObjectAsync(s3Settings.BucketName, key);
                return response.HttpStatusCode == HttpStatusCode.NoContent;
            }
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            throw HandleS3Exception(amazonS3Exception);
        }

        return false;
    }

    public async Task<StorageFileData> UploadFile(Stream stream, string key, CancellationToken cancellationToken)
    {
        try
        {
            var s3Settings = _s3Settings.Value;
            if (SettingsIsValid(s3Settings))
            {
                // high-level API to upload a file

                await _transferUtility.UploadAsync(stream, s3Settings.BucketName, key, cancellationToken);

                // an example of URL https://s3-eu-west-1.amazonaws.com/cmm.test/cmm.jpg
                return new StorageFileData
                {
                    Key = key,
                    Url = GeneratePreSignedURL(key),
                };
            }

            throw AwsNotSpecifiedException;
        }
        catch (AmazonS3Exception ex)
        {
            throw HandleS3Exception(ex);
        }
    }

    public async Task<ICollection<StorageFileData>> GetAllFiles(CancellationToken cancellationToken)
    {
        try
        {
            var s3Settings = _s3Settings.Value;
            if (SettingsIsValid(s3Settings))
            {
                var response = await _s3Client.ListObjectsAsync(s3Settings.BucketName, cancellationToken);

                var res = new List<StorageFileData>();
                foreach (var s3Object in response.S3Objects)
                {
                    var metadata = await _s3Client.GetObjectMetadataAsync(s3Settings.BucketName, s3Object.Key, cancellationToken);
                    res.Add(new StorageFileData
                    {
                        Key = s3Object.Key,
                        Url = GeneratePreSignedURL(s3Object.Key),
                        Size = s3Object.Size,
                        LastModifiedDate = s3Object.LastModified,
                        IsImage = metadata?.Headers.ContentType.Contains("image") ?? false
                    });
                }
                return res;
            }

            throw AwsNotSpecifiedException;
        }
        catch (AmazonS3Exception ex)
        {
            throw HandleS3Exception(ex);
        }
    }

    public async Task<ICollection<StorageFileData>> GetAllImages(CancellationToken cancellationToken)
    {
        var allFiles = await GetAllFiles(cancellationToken);
        return allFiles.Where(x => x.IsImage).ToList();
    }

    public async Task<StorageFileData> GetFile(string key, CancellationToken cancellationToken)
    {
        try
        {
            var s3Settings = _s3Settings.Value;
            if (SettingsIsValid(s3Settings))
            {
                var metadata = await _s3Client.GetObjectMetadataAsync(s3Settings.BucketName, key, cancellationToken);

                if (metadata is not null)
                {
                    return new StorageFileData
                    {
                        Key = key,
                        Url = GeneratePreSignedURL(key),
                        Size = metadata.ContentLength,
                        LastModifiedDate = metadata.LastModified,
                        IsImage = metadata.Headers.ContentType.Contains("image")
                    };
                }
            }
            return new StorageFileData();
        }
        catch (AmazonS3Exception ex)
        {
            throw HandleS3Exception(ex);
        }
    }

    public async Task<byte[]> DownloadFile(string key, CancellationToken cancellationToken)
    {
        try
        {
            var s3Settings = _s3Settings.Value;
            if (SettingsIsValid(s3Settings))
            {
                var s3Object = await _s3Client.GetObjectAsync(s3Settings.BucketName, key, cancellationToken);

                using var ms = new MemoryStream();
                s3Object.ResponseStream.CopyTo(ms);
                return ms.ToArray();
            }
            throw AwsNotSpecifiedException;
        }
        catch (AmazonS3Exception ex)
        {
            throw HandleS3Exception(ex);
        }
    }


    private static Exception HandleS3Exception(AmazonS3Exception exception)
    {
        if (exception.ErrorCode is not null &&
            (exception.ErrorCode.Equals("InvalidAccessKeyId") || exception.ErrorCode.Equals("InvalidSecurity")))
        {
            return new ConflictException("Please check the provided AWS Credentials.");
        }


        var message = exception.ErrorCode is not null &&
                         (exception.ErrorCode.Equals("InvalidAccessKeyId") || exception.ErrorCode.Equals("InvalidSecurity"))
                             ? "Please check the provided AWS Credentials."
                             : exception.Message;

        return new ConflictException(message);
    }

    private static string GetOpenUrl(AwsSettings settings, string key) =>
        $"{settings.S3Url}/{settings.BucketName}/{key}";

    private static bool SettingsIsValid(AwsSettings settings)
    {
        return settings is not null &&
            !(string.IsNullOrEmpty(settings.BucketName) || string.IsNullOrEmpty(settings.AccessKeyId) ||
              string.IsNullOrEmpty(settings.SecretAccessKey) || string.IsNullOrEmpty(settings.AwsRegion));
    }
}
