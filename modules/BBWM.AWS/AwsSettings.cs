namespace BBWM.AWS;
public class AwsSettings
{
    /// <summary>
    /// AWS Identity - Access key ID.
    /// </summary>
    public string AccessKeyId { get; set; }

    /// <summary>
    /// AWS Identity - Secret access key.
    /// </summary>
    public string SecretAccessKey { get; set; }

    /// <summary>
    /// Prefix that defines section related to the project.
    /// </summary>
    public string ParametersPath { get; set; }

    /// <summary>
    /// An interval in seconds for the configuration reloading period. If not specified then the configuration won't be reloaded.
    /// </summary>
    public int? ParametersReloadingInterval { get; set; }

    /// <summary>
    /// AWS Identity - AWS region.
    /// </summary>
    public string AwsRegion { get; set; }

    /// <summary>
    /// AWS S3 Bucket for files storage.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// AWS S3 url.
    /// </summary>
    public string S3Url => $"https://s3-{AwsRegion}.amazonaws.com";

    /// <summary>
    /// Defines whether AWS data protection (e.g. for storing antiforgery encryption keys) is enabled
    /// </summary>
    public bool DataProtectionEnabled { get; set; }
    /// <summary>
    /// Sets the unique name of this application within the data protection system.
    /// </summary>
    public string DataProtectionAppName { get; set; }
}