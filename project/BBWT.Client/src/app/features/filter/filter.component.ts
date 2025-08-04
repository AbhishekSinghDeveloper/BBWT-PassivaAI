import { Component, ContentChildren, EventEmitter, Input, Output, QueryList, TemplateRef } from "@angular/core";

import { SelectItem } from "primeng/api";

import { FilterInputType } from "./enums/filter-input-type";
import { FilterType, isCountableFilterType } from "./enums/filter-type";
import { CountableFilterMatchMode } from "./enums/countable-filter-match-mode";
import { StringFilterMatchMode } from "./enums/string-filter-match-mode";
import { IFilterInfoBase } from "./interfaces/filter-info-base";
import { IFilterSettings } from "./interfaces/filter-settings";
import { IFilterItem } from "./interfaces/filter-item";
import { QueryCommand } from "./classes/query-command";
import { FilterCustomTemplateDirective } from "./filter-custom-template.directive";


@Component({
    selector: "filter",
    templateUrl: "./filter.component.html",
    styleUrls: ["./filter.component.scss"]
})
export class FilterComponent {
    @Input() clearButtonText = "Clear";
    @Input() submitButtonText = "Search";
    readonly defaultDateFormat = "dd/mm/yy";
    readonly defaultCalendarYearRange = `1900:${new Date().getFullYear()}`;
    filters: {[key: string]: IFilterItem} = {};
    @Input() showClearButton = true;
    @Input() showSubmitButton = true;

    @ContentChildren(FilterCustomTemplateDirective) private customFiltersTemplates: QueryList<FilterCustomTemplateDirective>;


    @Input() set settings(value: IFilterSettings[]) {
        if (!value) return;
        this.replaceFilterSettings(value);
    }

    @Output() filter = new EventEmitter<IFilterInfoBase[]>();
    @Output() filterChange = new EventEmitter<IFilterItem>();


    get _filtersArray(): IFilterItem[] {
        return Object.keys(this.filters)
            .map(x => this.filters[x])
            .filter(x => !x.settings.visible || x.settings.visible())
            .sort((a, b) => a.settings.order - b.settings.order);
    }

    get _inputTypeEnum(): any {
        return FilterInputType;
    }

    get _matchModeEnum(): any {
        return CountableFilterMatchMode;
    }


    /** Clears all filter values and raises the filtering event if "performSearching" is "true". */
    clear(performSearching = true): void {
        this.replaceFilterSettings(Object.keys(this.filters).map(x => this.filters[x].settings));
        if (performSearching) this.search();
    }

    /** Creates the result of calling a filter event based on the current values. */
    getFilters(): IFilterInfoBase[] {
        const result: IFilterInfoBase[] = [];

        Object.keys(this.filters).forEach(key => {
            const filter = this.filters[key];

            if (filter.settings.visible && !filter.settings.visible()) return;

            if ((filter.value == null || filter.value === "") && !filter.settings.applyFilterIfNullValue) return;

            if (filter.matchMode == CountableFilterMatchMode.Between) {
                if (!filter.value[0] || !filter.value[1]) return;
            }

            if (filter.matchMode == CountableFilterMatchMode.In || filter.matchMode == StringFilterMatchMode.In) {
                if (!Array.isArray(filter.value) || !(<any[]> filter.value).length) return;
            }

            if (!filter.value && filter.settings.ignoreIfConvertibleToFalse) return;
            if (filter.value && filter.settings.ignoreIfConvertibleToTrue) return;

            let matchMode = filter.matchMode;
            if (filter.settings.inputType == FilterInputType.Multiselect) {
                switch (filter.settings.filterType) {
                    case FilterType.Numeric: matchMode = CountableFilterMatchMode.In; break;
                    default: matchMode = StringFilterMatchMode.In; break;
                }
            }

            const filterResult = QueryCommand.createFilter(
                key,
                filter.value,
                filter.settings.filterType ?? FilterType.Text,
                matchMode);

            if (filter.value == null) {
                filterResult["value"] = filter.value;
            }

            filterResult.id = filter.settings.id;

            result.push(filterResult);
        });
        return result;
    }

    /** Dynamically sets the options for the filter if it has a suitable 'FilterInputType'. */
    setOptions(field: string, options: SelectItem[]): void {
        const filterItem = this.filters[field];

        if (!filterItem) throw new Error(`A filter with the field name '${field}' does not exist.`);

        filterItem.settings.dropdownOptions = options;
    }

    /** Sets filter values directly and raises the filtering event if "performSearching" is "true". */
    setFilterValues(filters: Array<{ propertyName: string, value: any, matchMode?: CountableFilterMatchMode | StringFilterMatchMode }>, performSearch = true): void {
        filters.forEach(filter => {
            const filterItem = this.filters[filter.propertyName];

            if (filterItem) {
                filterItem.value = filter.value;
                filterItem.matchMode = filter.matchMode;
            }
        });

        if (performSearch) this.search();
    }

    /** Raises the filtering event directly. */
    search(): void {
        this.filter.emit(this.getFilters());
    }

    _customFilterExists(columnName: string): boolean {
        return this.customFiltersTemplates && this.customFiltersTemplates.some(x => x.filterCustomTemplate == columnName);
    }

    _getFilterTemplate(columnName: string): TemplateRef<any> {
        return this.customFiltersTemplates.find(x => x.filterCustomTemplate == columnName).template;
    }

    _matchModeSelectorVisible(filter: IFilterItem): boolean {
        return filter.settings.matchModeSelectorVisible !== false &&
            !this._customFilterExists(filter.field) &&
            filter.settings.inputType != FilterInputType.Dropdown &&
            filter.settings.inputType != FilterInputType.Checkbox &&
            filter.settings.inputType != FilterInputType.Multiselect;
    }

    _onMatchModeChanged(filter: IFilterItem): void {
        if (filter.matchMode == CountableFilterMatchMode.Between && !Array.isArray(filter.value)) {
            filter.value = null;
        }

        if (filter.matchMode != CountableFilterMatchMode.Between && Array.isArray(filter.value)) {
            filter.value = null;
        }

        if (filter.settings.autoSubmitSelectableInputChange) {
            this.search();
        }
    }

    _onFilterChanged(filterItem: IFilterItem): void {
        this.filterChange.emit(filterItem);
    }

    _onSelectableInputChanged(filter: IFilterItem): void {
        this._onFilterChanged(filter);

        if (filter.settings.autoSubmitSelectableInputChange) {
            this.search();
        }
    }


    private getDefaultMatchMode(filterType?: FilterType): CountableFilterMatchMode | StringFilterMatchMode {
        return isCountableFilterType(filterType) ? CountableFilterMatchMode.Equals : StringFilterMatchMode.Contains;
    }

    private getMatchModeOptions(filter: IFilterItem): SelectItem[] {
        if (filter.settings.filterType == FilterType.Numeric) {
            return <SelectItem[]>[
                { label: "Equal", value: CountableFilterMatchMode.Equals },
                { label: "Not equal", value: CountableFilterMatchMode.NotEquals },
                { label: "Less than", value: CountableFilterMatchMode.LessThan },
                { label: "Less than or equal to", value: CountableFilterMatchMode.LessThanOrEqual },
                { label: "Greater than", value: CountableFilterMatchMode.GreaterThan },
                { label: "Greater than or equal to", value: CountableFilterMatchMode.GreaterThanOrEqual },
                { label: "Between", value: CountableFilterMatchMode.Between }
            ];
        }
        if (filter.settings.filterType == FilterType.Date) {
            return <SelectItem[]>[
                { label: "Is", value: CountableFilterMatchMode.Equals },
                { label: "Is not", value: CountableFilterMatchMode.NotEquals },
                { label: "Before", value: CountableFilterMatchMode.LessThan },
                { label: "After", value: CountableFilterMatchMode.GreaterThan },
                { label: "Between", value: CountableFilterMatchMode.Between }
            ];
        }
        if (!filter.settings.filterType || filter.settings.filterType == FilterType.Text) {
            return <SelectItem[]>[
                { label: "Contains", value: StringFilterMatchMode.Contains },
                { label: "Not contains", value: StringFilterMatchMode.NotContains },
                { label: "Starts with", value: StringFilterMatchMode.StartsWith },
                { label: "Ends with", value: StringFilterMatchMode.EndsWith },
                { label: "Equal", value: StringFilterMatchMode.Equals },
                { label: "Not equal", value: StringFilterMatchMode.NotEquals }
            ];
        }
    }

    private checkFilter(filter: IFilterItem): void {
        if (filter.settings.defaultValue != null) {
            filter.value = filter.settings.defaultValue;
        }

        if (filter.settings.inputType == FilterInputType.Checkbox && filter.value == null) {
            filter.value = false;
        }

        if (!filter.matchMode) {
            filter.matchMode = this.getDefaultMatchMode(filter.settings.filterType);
        }

        if (!filter.settings.matchModeOptions) {
            filter.settings.matchModeOptions = this.getMatchModeOptions(filter);
        }

        if (filter.settings.inputType == FilterInputType.Multiselect) {
            filter.matchMode = CountableFilterMatchMode.In;
            filter.settings.matchModeOptions = [];
        }

        if (!filter.settings.dropdownOptions) {
            if (filter.settings.inputType == FilterInputType.Dropdown ||
                filter.settings.inputType == FilterInputType.Multiselect) {
                filter.settings.dropdownOptions = [];
            }
        }

        if (filter.settings.inputType == FilterInputType.Calendar) {
            if (!filter.settings.calendarYearRange) {
                filter.settings.calendarYearRange = `1900:${(new Date()).getFullYear()}`;
            }
        }

        if (filter.settings.header == null) {
            filter.settings.header = "";
        }
    }

    private replaceFilterSettings(settings: IFilterSettings[]): void {
        this.filters = {};
        settings.forEach(settingsItem => {
            this.filters[settingsItem.valueFieldName] = {
                field: settingsItem.valueFieldName,
                matchMode: settingsItem.matchMode || this.getDefaultMatchMode(settingsItem.filterType),
                settings: settingsItem,
                value: null
            } as IFilterItem;

            this.checkFilter(this.filters[settingsItem.valueFieldName]);
        });
    }
}