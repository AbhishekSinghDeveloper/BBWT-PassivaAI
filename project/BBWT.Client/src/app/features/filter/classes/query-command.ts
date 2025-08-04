import { FilterMatchMode, FilterMetadata, LazyLoadEvent } from "primeng/api";
import * as moment from "moment";

import { IQueryCommand } from "../interfaces/query-command";
import { IFilterInfoBase } from "../interfaces/filter-info-base";
import { StringFilterMatchMode } from "../enums/string-filter-match-mode";
import { CountableFilterMatchMode } from "../enums/countable-filter-match-mode";
import { FilterType } from "../enums/filter-type";
import { NumberBetweenFilter, NumberFilter } from "./number-filter";
import { DateBetweenFilter, DateFilter } from "./date-filter";
import { StringFilter } from "./string-filter";
import { BooleanFilter } from "./bool-filter";
import { NumberArrayFilter } from "./number-array-filter";
import { StringArrayFilter } from "./string-array-filter";


export class QueryCommand implements IQueryCommand {
    skip?: number;
    take?: number;
    sortingDirection?: number;
    sortingField?: string;
    filters?: IFilterInfoBase[];


    constructor(event: LazyLoadEvent, filterDataTypesMap?: {[key: string]: FilterType}) {
        this.skip = event.first;
        this.take = event.rows;
        this.sortingDirection = event.sortOrder;
        this.sortingField = event.sortField;

        if (event.filters) {
            this.filters = [];

            Object.keys(event.filters).forEach(key => {
                const metadata: FilterMetadata = event.filters[key];

                if (metadata.value == null) return;

                const filterType = filterDataTypesMap && filterDataTypesMap[key] != null
                    ? filterDataTypesMap[key]
                    : FilterType.Text;
                this.filters.push(QueryCommand.createFilter(
                    key,
                    metadata.value,
                    filterType,
                    filterType == FilterType.Text
                        ? this.convertMatchModeToStringMatchMode(metadata.matchMode)
                        : this.convertMatchModeToCountableMatchMode(metadata.matchMode)));
            });
        }
    }


    static createFilter(
        key: string,
        value: any,
        filterType: FilterType,
        matchMode: CountableFilterMatchMode | StringFilterMatchMode): IFilterInfoBase {
        switch (filterType) {
            case FilterType.Date:
                return matchMode == CountableFilterMatchMode.Between
                    ? new DateBetweenFilter(key, QueryCommand.tryParseToDate(value[0]), QueryCommand.tryParseToDate(value[1]))
                    : new DateFilter(key, QueryCommand.tryParseToDate(value), matchMode as CountableFilterMatchMode);
            case FilterType.Numeric:
                switch(<CountableFilterMatchMode> matchMode) {
                    case CountableFilterMatchMode.Between:
                        return new NumberBetweenFilter(
                            key,
                            QueryCommand.tryParseToNumeric(value[0]),
                            QueryCommand.tryParseToNumeric(value[1]));
                    case CountableFilterMatchMode.In:
                        return new NumberArrayFilter(key, (<any[]> value).map(x => QueryCommand.tryParseToNumeric(x)));
                    default:
                        return new NumberFilter(key, QueryCommand.tryParseToNumeric(value), matchMode as CountableFilterMatchMode);
                }
            case FilterType.Boolean:
                return new BooleanFilter(key, Boolean(value));
            default:
                switch(<StringFilterMatchMode> matchMode) {
                    case StringFilterMatchMode.In:
                        return new StringArrayFilter(key, value.map(x => String(x)));
                    default:
                        return new StringFilter(key, value, matchMode as StringFilterMatchMode);
                }
        }
    }

    static tryParseToDate(value: any): Date {
        if (value != null && Object.prototype.toString.call(value) !== "[object Date]") {
            const momentValue = moment(value);
            if (momentValue.isValid()) {
                return momentValue.toDate();
            } else {
                throw new Error("Impossible to parse value to the 'Date' type.");
            }
        }

        return value as Date;
    }

    static tryParseToNumeric(value: any): number {
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


    private convertMatchModeToCountableMatchMode(filterMatchMode: FilterMatchMode): CountableFilterMatchMode {
        switch (filterMatchMode) {
            case FilterMatchMode.NOT_EQUALS: case FilterMatchMode.IS_NOT: return CountableFilterMatchMode.NotEquals;
            case FilterMatchMode.LESS_THAN: case FilterMatchMode.BEFORE: return CountableFilterMatchMode.LessThan;
            case FilterMatchMode.LESS_THAN_OR_EQUAL_TO: return CountableFilterMatchMode.LessThanOrEqual;
            case FilterMatchMode.GREATER_THAN: case FilterMatchMode.AFTER: return CountableFilterMatchMode.GreaterThan;
            case FilterMatchMode.GREATER_THAN_OR_EQUAL_TO: return CountableFilterMatchMode.GreaterThanOrEqual;
            case FilterMatchMode.IN: return CountableFilterMatchMode.In;
            case FilterMatchMode.BETWEEN: return CountableFilterMatchMode.Between;
            default: return CountableFilterMatchMode.Equals;
        }
    }

    private convertMatchModeToStringMatchMode(filterMatchMode: FilterMatchMode): StringFilterMatchMode {
        switch (filterMatchMode) {
            case FilterMatchMode.EQUALS: return StringFilterMatchMode.Equals;
            case FilterMatchMode.NOT_EQUALS: return StringFilterMatchMode.NotEquals;
            case FilterMatchMode.STARTS_WITH: return StringFilterMatchMode.StartsWith;
            case FilterMatchMode.ENDS_WITH: return StringFilterMatchMode.EndsWith;
            case FilterMatchMode.NOT_CONTAINS: return StringFilterMatchMode.NotContains;
            case FilterMatchMode.IN: return StringFilterMatchMode.In;
            default: return StringFilterMatchMode.Contains;
        }
    }
}