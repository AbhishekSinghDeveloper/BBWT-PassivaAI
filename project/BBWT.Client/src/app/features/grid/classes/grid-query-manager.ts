import { QueryList, TemplateRef } from "@angular/core";

import { FilterMatchMode, FilterMetadata, LazyLoadEvent } from "primeng/api";
import { QueryOptions } from "odata-query";
import * as moment from "moment";

import {
    CountableFilterMatchMode,
    FilterInputType,
    FilterType,
    IFilterInfoBase,
    IFilterItem,
    isCountableFilterType,
    StringFilterMatchMode
} from "@features/filter";
import { GridComponent } from "../grid.component";
import { GridColumnTemplate } from "../grid-template.directive";


export class GridQueryManager {
    readonly dateDefaultMatchModeOptions = [
        { label: "Is", value: FilterMatchMode.IS },
        { label: "Is not", value: FilterMatchMode.IS_NOT },
        { label: "Before", value: FilterMatchMode.BEFORE },
        { label: "After", value: FilterMatchMode.AFTER },
        { label: "Between", value: FilterMatchMode.BETWEEN }
    ];


    constructor(private _grid: GridComponent, private _columnTemplates: QueryList<GridColumnTemplate>) {}


    get filterTypeEnum(): any {
        return FilterType;
    }

    get filterInputTypeEnum(): any {
        return FilterInputType; 
    }

    get filterMatchModeEnum(): any {
        return FilterMatchMode; 
    }


    checkLazyLoadEvent(lazyLoadEvent: LazyLoadEvent): void {
        Object.keys(lazyLoadEvent.filters).forEach(key => {
            const matchMode = lazyLoadEvent.filters[key].matchMode;
            const value = lazyLoadEvent.filters[key].value;
            if ((matchMode == FilterMatchMode.BETWEEN  || matchMode == FilterMatchMode.IN) && !Array.isArray(value)) {
                lazyLoadEvent.filters[key].value = null;
                (this._grid._table.filters[key] as FilterMetadata).value = null;
            }
        });

        const checkedFilters: {[s: string]: FilterMetadata} = {};
        Object.keys(lazyLoadEvent.filters).forEach(key => {
            const value = lazyLoadEvent.filters[key].value;
            if (!this._grid._filterComponent) {
                const column = this._grid._table.columns.find(x => x.field == key);

                if (value == null && !column.filterSettings?.applyFilterIfNullValue) return;
                if (column.filterSettings?.ignoreIfConvertibleToTrue && value) return;
                if (column.filterSettings?.ignoreIfConvertibleToFalse !== false && !value) return;

                let resultFilterKey = key;
                if (column.serverFieldName != null) resultFilterKey = column.serverFieldName;

                checkedFilters[resultFilterKey] = column.filterSettings?.transform == null
                    ? lazyLoadEvent.filters[key]
                    : column.filterSettings?.transform(lazyLoadEvent.filters[key]);

            } else {
                checkedFilters[key] = lazyLoadEvent.filters[key];
            }
        });
        lazyLoadEvent.filters = checkedFilters;

        if (lazyLoadEvent.sortField != null) {
            const column = this._grid._table.columns.find(x => x.field == lazyLoadEvent.sortField);
            if (column?.serverFieldName != null) lazyLoadEvent.sortField = column.serverFieldName;
        }
    }

    customFilterExists(columnName: string): boolean {
        return this._columnTemplates
            .filter(x => x.gridColumnTemplate == "filterView")
            .some(x => x.columnName == columnName);
    }

    getFilterTemplate(columnName: string): TemplateRef<any> {
        const columnTemplate = this._columnTemplates
            .filter(x => x.gridColumnTemplate == "filterView")
            .find(x => x.columnName == columnName);

        if (!columnTemplate) {
            throw Error(`Custom column filter template is not defined for a column '${columnName}' - one is required for InputType.CustomTemplate for the column's filterSettings`);
        }

        return columnTemplate ? columnTemplate.template : null;
    }

    getLazyLoadMetadata(): LazyLoadEvent {
        const result = this._grid._table.createLazyLoadMetadata();
        this.checkLazyLoadEvent(result);
        return result;
    }

    initFilterComponent(): void {
        if (!this._grid._filterComponent) return;

        this._grid._filterComponent.filter.subscribe(filters => {
            this.passCrudFiltersToTable(filters);
        });

        const newFilterValues: Array<{ propertyName: string, value: any, matchMode?: CountableFilterMatchMode | StringFilterMatchMode }> = [];
        Object.keys(this._grid._table.filters).forEach(filterMetadataKey => {
            const filterMetadata = this._grid._table.filters[filterMetadataKey] as FilterMetadata;
            const filterItem = this._grid._filterComponent.filters[filterMetadataKey];
            if (filterItem) {
                newFilterValues.push(this.getFilterValue(filterMetadataKey, filterMetadata, filterItem));
            }
        });

        Object.keys(this._grid._filterComponent.filters)
            .filter(x =>
                this._grid._filterComponent.filters[x].settings.defaultValue != null &&
                newFilterValues.every(y => y.propertyName != x))
            .forEach(x => {
                if (this._grid._table.stateRestored) {
                    newFilterValues.push({
                        propertyName: x,
                        value: null
                    });
                } else {
                    newFilterValues.push(
                        this.getFilterValue(x, null, this._grid._filterComponent.filters[x])
                    );
                }
            });

        this._grid._filterComponent.setFilterValues(newFilterValues, true);
    }

    passCrudFiltersToTable(crudFilters: IFilterInfoBase[], forceUpdate = true, replaceAllList = true): void {
        if (replaceAllList) {
            this._grid._table.filters = {};
        }

        crudFilters.forEach(crudFilterItem => {
            const filterMatchMode = this.getFilterMatchModeByCrudFilter(crudFilterItem);
            const value = filterMatchMode != FilterMatchMode.BETWEEN
                ? crudFilterItem["value"]
                : [crudFilterItem["from"], crudFilterItem["to"]];
            this._grid._table.filters[crudFilterItem.propertyName] = {
                value: value,
                matchMode: filterMatchMode
            } as FilterMetadata;
        });

        this._grid.trySaveState();

        if (forceUpdate) {
            this._grid._table._filter();
        }
    }

    passCrudFiltersToODataQuery<T = any>(crudFilters: IFilterInfoBase[], existingQuery?: Partial<QueryOptions<T>>): Partial<QueryOptions<T>> {
        if (!existingQuery) existingQuery = {};

        if (!this._grid?._table.columns) return existingQuery;

        existingQuery.filter = crudFilters.map((crudFilter: any) => {
            let filterOperator = "eq";

            if (crudFilter.matchMode == StringFilterMatchMode.Contains) {
                filterOperator = "contains";
            }
            if (crudFilter.matchMode == StringFilterMatchMode.StartsWith) {
                filterOperator = "startswith";
            }
            if (crudFilter.matchMode == StringFilterMatchMode.EndsWith) {
                filterOperator = "endswith";
            }
            if (crudFilter.matchMode == CountableFilterMatchMode.LessThan) {
                filterOperator = "lt";
            }
            if (crudFilter.matchMode == CountableFilterMatchMode.LessThanOrEqual) {
                filterOperator = "le";
            }
            if (crudFilter.matchMode == CountableFilterMatchMode.GreaterThan) {
                filterOperator = "gt";
            }
            if (crudFilter.matchMode == CountableFilterMatchMode.GreaterThanOrEqual) {
                filterOperator = "ge";
            }

            const filter = {};
            filter[crudFilter.propertyName] = {};
            filter[crudFilter.propertyName][filterOperator] = crudFilter.value;
            return filter;
        });

        return existingQuery;
    }

    passGridColumnsToODataSelectExpandQuery<T = any>(existingQuery?: Partial<QueryOptions<T>>): Partial<QueryOptions<T>> {
        if (!existingQuery) existingQuery = {};

        if (!this._grid._table.columns?.length) return existingQuery;

        if (!existingQuery.select) existingQuery.select = [];
        this._grid._table.columns.forEach(columnItem => {
            const segments = columnItem.field.split(".");
            let nestedOptions = existingQuery;
            const segmentsLength = columnItem.expand ? segments.length : segments.length - 1;
            for (let index = 0; index < segmentsLength; index++) {
                if (!nestedOptions.expand) nestedOptions.expand = {};
                if (!nestedOptions.expand[segments[index]]) {
                    nestedOptions.expand[segments[index]] = {};
                }
                nestedOptions = nestedOptions.expand[segments[index]];
            }

            if (!columnItem.expand) {
                if (!nestedOptions.select) nestedOptions.select = [];
                (nestedOptions.select as any).push(segments[segments.length - 1]);
            }
        });

        return existingQuery;
    }

    passLazyLoadMetadataToODataQuery<T = any>(event: LazyLoadEvent, existingQuery?: Partial<QueryOptions<T>>): Partial<QueryOptions<T>> {
        if (!existingQuery) existingQuery = {};

        if (event.rows > 0) {
            existingQuery.top = event.rows;
        }

        if (event.first > 0) {
            existingQuery.skip = event.first;
        }

        if (event.sortField) {
            existingQuery.orderBy = `${event.sortField} ${event.sortOrder < 0 ? "desc" : "asc"}`;
        }

        if (event.filters) {
            const filters = [];

            Object.getOwnPropertyNames(event.filters).forEach(propertyName => {
                const filterMetadata = event.filters[propertyName] as FilterMetadata;
                propertyName = propertyName.replace(".", "/");

                let matchMode = filterMetadata.matchMode?.toLowerCase() || "contains";
                switch (matchMode) {
                    case FilterMatchMode.EQUALS:
                    case FilterMatchMode.IS:
                        matchMode = "eq";
                        break;
                    case FilterMatchMode.NOT_EQUALS:
                    case FilterMatchMode.IS_NOT:
                    case FilterMatchMode.DATE_IS_NOT:
                        matchMode = "ne";
                    break;
                    case FilterMatchMode.GREATER_THAN_OR_EQUAL_TO:
                    case FilterMatchMode.AFTER:
                    case FilterMatchMode.DATE_AFTER:
                        matchMode = "ge";
                        break;
                    case FilterMatchMode.LESS_THAN_OR_EQUAL_TO:
                    case FilterMatchMode.BEFORE:
                    case FilterMatchMode.DATE_BEFORE:
                        matchMode = "le";
                        break;
                }

                let filter: any;
                switch (matchMode) {
                    case "eq": case "ne": case "gt": case "ge": case "lt": case "le": case "in":
                    case "startswith": case "endswith": case "contains":
                        filter = {};
                        filter[propertyName] = {};
                        filter[propertyName][matchMode] = filterMetadata.value;
                        filters.push(filter);
                        break;
                    // case "length": case "tolower": case "toupper": case "trim":
                    // case "day": case "month": case "year": case "hour": case "minute": case "second":
                    // case "round": case "floor": case "ceiling":
                    //     filter = {};
                    //     const filterFunction1 = `${matchMode}(${propertyName})`;
                    //     filter[filterFunction1] = {};
                    //     filter[filterFunction1][matchModeArray[1]] = filterMetadata.value;
                    //     break;
                    // case "substring":
                    //     filter = {};
                    //     let filterFunction2 = `${matchModeArray[0]}(${propertyName}, ${matchModeArray[1]}`;
                    //     if (matchModeArray.length == 4) filterFunction2 += `, ${matchModeArray[2]}`;
                    //     filterFunction2 += ")";
                    //     filter[filterFunction2] = {};
                    //     filter[filterFunction2][matchModeArray[matchModeArray.length - 1]] = filterMetadata.value;
                    //     break;
                    // case "indexof":
                    //     filter = {};
                    //     const filterFunction3 = `${matchModeArray[0]}(${propertyName}, "${matchModeArray[1]}")`;
                    //     filter[filterFunction3] = {};
                    //     filter[filterFunction3][matchModeArray[2]] = filterMetadata.value;
                    //     break;
                }
            });

            existingQuery.filter = filters;
        }

        return existingQuery;
    }


    private checkFilterValue(value: any, filterSettings: IFilterItem): any {
        switch (filterSettings.settings.filterType) {
            case FilterType.Boolean: return Boolean(value);
            case FilterType.Numeric:
                if (filterSettings.matchMode == CountableFilterMatchMode.Between ||
                    filterSettings.matchMode == CountableFilterMatchMode.In) {
                    if (Array.isArray(value)) {
                        value.forEach((val, index) => value[index] = this.tryParseToNumeric(val));
                        return value;
                    } else {
                        return null;
                    }
                } else {
                    return this.tryParseToNumeric(value);
                }
            case FilterType.Date:
                if (filterSettings.matchMode == CountableFilterMatchMode.Between ||
                    filterSettings.matchMode == CountableFilterMatchMode.In) {
                    if (Array.isArray(value)) {
                        value.forEach((val, index) => value[index] = this.tryParseToDate(val));
                        return value;
                    } else {
                        return null;
                    }
                } else {
                    return this.tryParseToDate(value);
                }
            default:
                if (filterSettings.matchMode == StringFilterMatchMode.In) {
                    if (Array.isArray(value)) {
                        value.forEach((val, index) => value[index] = String(val));
                        return value;
                    } else {
                        return null;
                    }
                } else {
                    return value != null ? String(value) : null;
                }
        }
    }

    private getFilterTypeByCrudFilter(filterInfo: IFilterInfoBase): FilterType {
        switch (filterInfo.$type) {
            case "string": return FilterType.Text;
            case "number": case "numberBetween": return FilterType.Numeric;
            case "date": case "dateBetween": return FilterType.Date;
            case "boolean": return FilterType.Boolean;
        }
    }

    private getFilterMatchModeByCrudFilter(filterInfo: IFilterInfoBase): FilterMatchMode {
        if (filterInfo.$type == "numberBetween" || filterInfo.$type == "dateBetween") {
            return FilterMatchMode.BETWEEN;
        }

        if (filterInfo.$type == "numberArray" || filterInfo.$type == "stringArray") {
            return FilterMatchMode.IN;
        }

        if (filterInfo.$type == "number" || filterInfo.$type == "date") {
            switch (filterInfo["matchMode"]) {
                case CountableFilterMatchMode.NotEquals: return FilterMatchMode.NOT_EQUALS;
                case CountableFilterMatchMode.GreaterThan: return FilterMatchMode.GREATER_THAN;
                case CountableFilterMatchMode.GreaterThanOrEqual: return FilterMatchMode.GREATER_THAN_OR_EQUAL_TO;
                case CountableFilterMatchMode.LessThan: return FilterMatchMode.LESS_THAN;
                case CountableFilterMatchMode.LessThanOrEqual: return FilterMatchMode.LESS_THAN_OR_EQUAL_TO;
            }
        }

        if (filterInfo.$type == "string") {
            switch (filterInfo["matchMode"]) {
                case StringFilterMatchMode.Equals: return FilterMatchMode.EQUALS;
                case StringFilterMatchMode.NotEquals: return FilterMatchMode.NOT_EQUALS;
                case StringFilterMatchMode.EndsWith: return FilterMatchMode.ENDS_WITH;
                case StringFilterMatchMode.StartsWith: return FilterMatchMode.STARTS_WITH;
                case StringFilterMatchMode.NotContains: return FilterMatchMode.NOT_CONTAINS;
                default: return FilterMatchMode.CONTAINS;
            }
        }

        return FilterMatchMode.EQUALS;
    }

    private getFilterValue(filterKey: string, filterMetadata: FilterMetadata, filterItem: IFilterItem): any {
        const filterValue = this.checkFilterValue(filterMetadata?.value || filterItem.value, filterItem);
        const filterMatchMode = filterMetadata?.matchMode != null ? filterMetadata.matchMode : filterItem.settings.matchMode;

        const newFilterValue = <any>{
            propertyName: filterKey,
            value: filterValue
        };
        const isCountableMatchMode = isCountableFilterType(filterItem.settings.filterType);
        switch (filterItem.settings.inputType) {
            case FilterInputType.Dropdown:
                newFilterValue.matchMode = filterMatchMode
                    ?? (isCountableMatchMode ? CountableFilterMatchMode.Equals : StringFilterMatchMode.Equals);
                break;
            case FilterInputType.Multiselect:
                newFilterValue.matchMode = filterMatchMode
                    ?? (isCountableMatchMode ? CountableFilterMatchMode.In : StringFilterMatchMode.In);
                break;
            default:
                switch (filterItem.settings.filterType) {
                    case FilterType.Boolean:
                        break;
                    case FilterType.Date: case FilterType.Numeric:
                        switch (filterMatchMode) {
                            case FilterMatchMode.NOT_EQUALS: newFilterValue.matchMode = CountableFilterMatchMode.NotEquals; break;
                            case FilterMatchMode.LESS_THAN: newFilterValue.matchMode = CountableFilterMatchMode.LessThan; break;
                            case FilterMatchMode.LESS_THAN_OR_EQUAL_TO: newFilterValue.matchMode = CountableFilterMatchMode.LessThanOrEqual; break;
                            case FilterMatchMode.GREATER_THAN: newFilterValue.matchMode = CountableFilterMatchMode.GreaterThan; break;
                            case FilterMatchMode.GREATER_THAN_OR_EQUAL_TO: newFilterValue.matchMode = CountableFilterMatchMode.GreaterThanOrEqual; break;
                            case FilterMatchMode.BETWEEN: newFilterValue.matchMode = CountableFilterMatchMode.Between; break;
                            default: newFilterValue.matchMode = CountableFilterMatchMode.Equals; break;
                        }
                        break;
                    case FilterType.Text:
                        switch (filterMatchMode) {
                            case FilterMatchMode.EQUALS: newFilterValue.matchMode = StringFilterMatchMode.Equals; break;
                            case FilterMatchMode.NOT_EQUALS: newFilterValue.matchMode = StringFilterMatchMode.NotEquals; break;
                            case FilterMatchMode.STARTS_WITH: newFilterValue.matchMode = StringFilterMatchMode.StartsWith; break;
                            case FilterMatchMode.ENDS_WITH: newFilterValue.matchMode = StringFilterMatchMode.EndsWith; break;
                            case FilterMatchMode.NOT_CONTAINS: newFilterValue.matchMode = StringFilterMatchMode.NotContains; break;
                            default: newFilterValue.matchMode = StringFilterMatchMode.Contains; break;
                        }
                        break;
                }
                break;
        }

        return newFilterValue;
    }

    private tryParseToDate(value: any): Date {
        if (value == null) return value;

        if (Object.prototype.toString.call(value) !== "[object Date]") {
            const momentValue = moment(value);
            if (momentValue.isValid()) {
                return momentValue.toDate();
            } else {
                throw new Error("Impossible to parse value to the 'Date' type.");
            }
        }

        return value as Date;
    }

    private tryParseToNumeric(value: any): number {
        if (value == null) return value;

        if (typeof value !== "number") {
            const numberValue = Number(value);
            if (!isNaN(numberValue)) {
                return numberValue;
            } else {
                throw new Error("Impossible to parse value to the 'Number' type.");
            }
        }

        return value as number;
    }
}