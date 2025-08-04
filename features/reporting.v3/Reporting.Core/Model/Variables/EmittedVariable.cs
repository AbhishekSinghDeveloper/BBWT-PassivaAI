using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.ModelJsonConverters;
using Newtonsoft.Json;

namespace BBF.Reporting.Core.Model.Variables;

[JsonConverter(typeof(EmittedVariableJsonConverter))]
public abstract class EmittedVariable
{
    public string Name { get; set; } = null!;
    public bool Empty { get; set; }
    public EmittedVariableBehavior BehaviorOnEmpty { get; set; }
}

public class EmittedVariable<T> : EmittedVariable
{
    public T? Value { get; set; } = default!;
}