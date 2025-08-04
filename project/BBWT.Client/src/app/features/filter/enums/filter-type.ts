export enum FilterType {
    Boolean = "boolean",
    Date = "date",
    Numeric = "numeric",
    Text = "text"
}

export function isCountableFilterType(filterType: FilterType): boolean {
    return filterType == FilterType.Numeric || filterType == FilterType.Date;
}

export function isStringFilterType(filterType: FilterType): boolean {
    return filterType == FilterType.Text;
}
