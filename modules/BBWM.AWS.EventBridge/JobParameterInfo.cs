namespace BBWM.AWS.EventBridge;

public sealed class JobParameterInfo
{
    public string Name { get; set; }

    public bool Required { get; set; }

    public string Description { get; set; }
}
