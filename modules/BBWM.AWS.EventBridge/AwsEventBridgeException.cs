namespace BBWM.AWS.EventBridge;

[Serializable]
public class AwsEventBridgeException : Exception
{
    public AwsEventBridgeException() { }
    public AwsEventBridgeException(string message) : base(message) { }
    public AwsEventBridgeException(string message, Exception inner) : base(message, inner) { }
    protected AwsEventBridgeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
