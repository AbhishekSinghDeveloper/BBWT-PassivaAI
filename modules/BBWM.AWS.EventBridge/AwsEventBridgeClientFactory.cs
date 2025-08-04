using Amazon;
using Amazon.EventBridge;

using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Exceptions;

using Microsoft.Extensions.Options;

namespace BBWM.AWS.EventBridge;

public class AwsEventBridgeClientFactory : IAwsEventBridgeClientFactory
{
    private readonly AwsSettings _awsSettings;

    public AwsEventBridgeClientFactory(IOptionsSnapshot<AwsSettings> awsSettings)
    {
        _awsSettings = awsSettings.Value;
    }

    public IAmazonEventBridge CreateClient()
    {
        if (_awsSettings is null ||
            string.IsNullOrEmpty(_awsSettings.AwsRegion) ||
            string.IsNullOrEmpty(_awsSettings.AccessKeyId) ||
            string.IsNullOrEmpty(_awsSettings.SecretAccessKey))
        {
            throw new ConflictException("AWS settings is not set.");
        }

        return new AmazonEventBridgeClient(
            _awsSettings.AccessKeyId,
            _awsSettings.SecretAccessKey,
            RegionEndpoint.GetBySystemName(_awsSettings.AwsRegion));
    }
}
