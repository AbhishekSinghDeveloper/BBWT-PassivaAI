export enum CountableFilterMatchMode {
    Equals = 0,
    NotEquals = 1,
    LessThan = 2,
    LessThanOrEqual = 3,
    GreaterThan = 4,
    GreaterThanOrEqual = 5,

    /* Do not use this value for ISimpleFilterInfoBase<T> filters.
    This value is for the IFilterSettings interface only to distinguish a "Simple" with a "Between" filters. */
    Between = 6,
    In = 7
}
