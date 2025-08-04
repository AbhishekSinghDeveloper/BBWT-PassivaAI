export interface IFilterInfoBase {
    id?: string;

    propertyName: string;

    /*
    * It is important that the value of this field
    * matches the substring of the class name
    * that represents this filter on the backend.
    * Otherwise, model binding may fail.
    *
    * Examples:
    *   for "NumberFilter" we should use the "number" (case insensitive) value
    *   for "NumberBetweenFilter" we should use the "numberBetween" (case insensitive) value.
    *
    * See the FilterInfoModelBinder class.
    */
    $type: string;
}

export interface ISimpleFilterInfoBase<T = any> extends IFilterInfoBase {
    value: T;
}
