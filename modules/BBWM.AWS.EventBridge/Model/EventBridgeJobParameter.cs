namespace BBWM.AWS.EventBridge.Model;

public class EventBridgeJobParameter
{
    public string Name { get; set; }

    public string Value { get; set; }

    public override int GetHashCode() => $"{Name}=>{Value}".GetHashCode();
}
