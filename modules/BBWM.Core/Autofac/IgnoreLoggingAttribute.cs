namespace BBWM.Core.Autofac;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class IgnoreLoggingAttribute : Attribute
{
    public bool? JustIgnoreArgumentsLogging { get; }

    public bool? JustIgnoreReturnValueLogging { get; }

    public bool IgnoreEntireLogging { get; }

    public IgnoreLoggingAttribute(bool justIgnoreArgumentsLogging, bool justIgnoreReturnValueLogging = false)
    {
        JustIgnoreReturnValueLogging = justIgnoreReturnValueLogging;
        JustIgnoreArgumentsLogging = justIgnoreArgumentsLogging;
        IgnoreEntireLogging = false;
    }

    public IgnoreLoggingAttribute()
        => IgnoreEntireLogging = true;
}
