using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;

using AspNetCore.DataProtection.Aws.S3;

using BBWM.AppConfiguration;
using BBWM.FileStorage;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BBWM.AWS;

public static class ServiceCollectionExtensions
{
    public static void ConfigureAws(this IServiceCollection services, IConfiguration configuration, string awsSectionName = "AwsSettings")
    {
        var awsConfigSection = configuration.GetSection(awsSectionName);
        var awsSettings = awsConfigSection.Get<AwsSettings>();
        // Default empty AWS options
        var awsOptions = new AWSOptions() { Region = RegionEndpoint.EUNorth1 };

        if (awsSettings != null &&
            !string.IsNullOrEmpty(awsSettings.AwsRegion) && !string.IsNullOrEmpty(awsSettings.AccessKeyId) &&
            !string.IsNullOrEmpty(awsSettings.SecretAccessKey))
        {
            awsOptions = new AWSOptions
            {
                Region = RegionEndpoint.GetBySystemName(awsSettings.AwsRegion),
                Credentials = new BasicAWSCredentials(awsSettings.AccessKeyId, awsSettings.SecretAccessKey),
            };
        }

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();

        services.AddScoped<IAwsService, AwsService>();
        services.AddScoped<IFileStorageProvider, AwsFileStorageProvider>();
        services.AddScoped<IAppConfigurationService, AwsAppConfigurationService>();
        services.Configure<AwsSettings>(awsConfigSection);
        services.AddAwsDataProtection(awsConfigSection);
        services.AddScoped<ITransferUtility>(sp =>
        {
            var s3Settings = sp.GetRequiredService<IOptionsSnapshot<AwsSettings>>().Value;
            var region = RegionEndpoint.GetBySystemName(s3Settings.AwsRegion);
            return new TransferUtility(s3Settings.AccessKeyId, s3Settings.SecretAccessKey, region);
        });
    }

    /// <summary>
    /// This option is related to load balancing and sharing the same configuration between multiple servers.
    /// Here we use AWS S3 as the encryption keys storage provider.
    /// </summary>
    /// <remarks>
    /// When the data protection system is initialized it applies some default settings based on the operational environment.
    /// These settings are generally good for applications running on a single machine. There are some cases where a developer
    /// may want to change these (perhaps because his application is spread across multiple machines or for compliance reasons),
    /// and for these scenarios the data protection system offers a rich configuration API.
    /// Some set up examples (common, not AWS related):
    /// https://jakeydocs.readthedocs.io/en/latest/security/data-protection/configuration/overview.html
    /// </remarks>
    private static void AddAwsDataProtection(this IServiceCollection services, IConfigurationSection awsConfigSection)
    {
        var awsSettings = awsConfigSection.Get<AwsSettings>();

        if (awsSettings is not null && awsSettings.DataProtectionEnabled &&
            !string.IsNullOrEmpty(awsSettings.AwsRegion) && !string.IsNullOrEmpty(awsSettings.AccessKeyId) &&
            !string.IsNullOrEmpty(awsSettings.SecretAccessKey) && !string.IsNullOrEmpty(awsSettings.BucketName))
        {
            var awsConfiguration = new S3XmlRepositoryConfig(awsSettings.BucketName)
            {
                KeyPrefix = "appKeys/"
            };

            var region = RegionEndpoint.GetBySystemName(awsSettings.AwsRegion);
            var s3Client = new AmazonS3Client(awsSettings.AccessKeyId, awsSettings.SecretAccessKey, region);

            var appName = awsSettings.DataProtectionAppName;
            services.AddDataProtection()
                .PersistKeysToAwsS3(s3Client, awsConfiguration)
                .SetApplicationName(string.IsNullOrEmpty(appName) ? "BBWT3" : appName);
        }
    }
}
