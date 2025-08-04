using System.Text.Json.Serialization;

namespace BBWM.Core.Filters;

[JsonConverter(typeof(FilterInfoBaseJsonConverter))]
public abstract class FilterInfoBase
{
    public int? Id { get; set; }

    public string PropertyName { get; set; }
}

public abstract class FilterInfoBase<T> : FilterInfoBase
{
    public T Value { get; set; }
}