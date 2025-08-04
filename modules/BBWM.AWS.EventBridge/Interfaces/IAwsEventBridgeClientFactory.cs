using Amazon.EventBridge;

namespace BBWM.AWS.EventBridge.Interfaces;

public interface IAwsEventBridgeClientFactory
{
    IAmazonEventBridge CreateClient();
}
