namespace BBWM.AWS.EventBridge;

public class AwsEventBridgeSettings
{
    public const string CONFIG_SECTION = "AwsEventBridgeSettings";

    public string APIKey { get; set; }

    public string TargetRoleArn { get; set; }

    public string AuthHeader { get; set; } = "X-Aws-Event-Bridge-Api-Key";

    public string ApiConnectionName { get; set; }

    public string ApiDestinationName { get; set; }
}
