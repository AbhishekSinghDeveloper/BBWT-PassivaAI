namespace BBWM.Core.Filters;


public enum CountableFilterMatchMode
{
    Equals = 0,
    NotEquals,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    In
}


public abstract class CountableFilterBase<T> : FilterInfoBase<T>
{
    public CountableFilterMatchMode MatchMode { get; set; }
}

public abstract class CountableBetweenFilterBase<T> : FilterInfoBase
{
    public T From { get; set; }

    public T To { get; set; }
}
