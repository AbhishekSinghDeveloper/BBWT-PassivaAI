namespace BBWM.AWS.EventBridge.Test;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class TestPriorityAttribute : Attribute
{
    public TestPriorityAttribute(int priority = 0) => Priority = priority;

    public int Priority { get; }
}
