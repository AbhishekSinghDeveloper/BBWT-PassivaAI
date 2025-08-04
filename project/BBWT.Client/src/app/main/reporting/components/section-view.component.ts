import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from "@angular/core";
import { DomSanitizer, SafeHtml } from "@angular/platform-browser";

import * as moment from "moment";

import {
    CountableFilterMatchMode,
    FilterComponent,
    FilterInputType,
    FilterType,
    IFilterSettings,
    IQueryCommand,
    NumberFilter,
    StringFilterMatchMode
} from "@features/filter";
import {
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    GridHelper,
    IGridColumn,
    IGridSettings,
    ITableSettings
} from "@features/grid";
import { ClrTypeGroup } from "@main/dbdoc";
import {
    IMasterSectionBinding,
    IMasterSectionEvent,
    ISectionDisplayView,
    ISectionViewColumn,
    ISectionViewFilter,
    QueryRuleCode
} from "../reporting-models";
import { SectionService } from "../services/section.service";


interface IColumnTemplate {
    column: IGridColumn;
    customTemplate: string;
}

@Component({
    selector: "section-view",
    templateUrl: "./section-view.component.html",
    styleUrls: ["./section-view.component.scss"]
})
export class SectionViewComponent implements OnInit {
    @Input() sectionId: string;
    @Input() forceSectionVisible: boolean;
    @Output() masterSectionCommand = new EventEmitter<IMasterSectionEvent>();
    @Output() sectionContainerStateChange = new EventEmitter<{
        sectionId: string,
        visible: boolean,
        expanded: boolean
    }>();

    protected _sectionContentVisible: boolean;
    public ShowSectionContent() {
        this._sectionContentVisible = true;
    }

    _sectionViewLoading: boolean;
    _sectionView: ISectionDisplayView;
    _viewedFilterSettings: IFilterSettings[] = [];
    _tableSettings: ITableSettings;
    _gridSettings: IGridSettings;
    _columnTemplates: IColumnTemplate[];
    _footerVisible: boolean;
    _aggregatedValues: {[key: string]: { value: string, textAlign: string }};

    private _masterSectionRowSelectEventRowData: any;
    private _masterSectionRowSelectEventBinding: IMasterSectionBinding;

    @ViewChild("filter", {static: false}) private _filter: FilterComponent;
    @ViewChild("grid", {static: false}) private _grid: GridComponent;


    constructor(private sanitizer: DomSanitizer, private sectionService: SectionService) {}


    ngOnInit(): void {
        this.refresh();
    }


    async refresh(): Promise<void> {
        this._sectionViewLoading = true;
        await this.handleSectionView(await this.sectionService.getDisplayView(this.sectionId));
        this._sectionViewLoading = false;
        setTimeout(() => this.loadFilterOptions(), 10);
    }


    private requestData(queryCommand: IQueryCommand): Promise<any[]> {
        if (this._masterSectionRowSelectEventRowData) {
            //let filterType = this._masterSectionRowSelectEventBinding.filterDataType;
            //let matchMode = this.getMatchModeByViewFilter(this._masterSectionRowSelectEventBinding.filterQueryRuleCode, filterType);

            // TODO: it should be an any type filter in the future because we may filter by any column of master-section's grid
            const newFilter = new NumberFilter(
                this._masterSectionRowSelectEventBinding.filterId,
                this._masterSectionRowSelectEventRowData[this._masterSectionRowSelectEventBinding.columnId]);

            queryCommand.filters.push(newFilter);
        }

        this.sectionService.getTotal(this.sectionId, queryCommand)
            .then(total => this._grid.setTableProperty("totalRecords", total));

        if (this._footerVisible) {
            this.sectionService.getAggregations(this.sectionId, queryCommand)
                .then(result => this._aggregatedValues = this.createFooterOutput(result));
        }

        return this.sectionService.getData(this.sectionId, queryCommand);
    }

    private handleSectionView(sectionView: ISectionDisplayView): void {
        this._sectionView = sectionView;

        const subscribedToRowSelect = this.isSubscribedToMasterGridRowSelected(sectionView);
        const initiallyCollapsed = sectionView.expandBehaviour === "initiallyCollapsed";

        this._sectionContentVisible = this.forceSectionVisible || !(subscribedToRowSelect || initiallyCollapsed);

        const sectionContainerVisible = !subscribedToRowSelect;
        const sectionContainerExpanded = subscribedToRowSelect || !initiallyCollapsed;
        this.emitSectionContainerStateChange(sectionContainerVisible, sectionContainerExpanded);

        this._footerVisible = sectionView.columns.some(x => x.footer?.expressions?.length);

        this._viewedFilterSettings = this.createFilterControls(sectionView.filters, sectionView.columns);

        this._tableSettings = {
            autoLayout: true,
            sortField: sectionView.defaultSortColumn,
            sortOrder: sectionView.defaultSortOrder,
            styleClass: "p-datatable-gridlines p-datatable-striped",
            selectionMode: "single",
            dataKey: null
        };
        this.createGridColumnSettings(sectionView.columns);

        this._gridSettings = {
            readonly: true,
            dataService: this,
            dataServiceGetPageMethodName: "requestData",
            visibleColumnsSelector: sectionView.showVisibleColumnsSelector,
            exportEnabled: true
        };
    }

    private createFilterControls(sectionViewFilters: ISectionViewFilter[], columnViews: ISectionViewColumn[]): IFilterSettings[] {
        const result = [];

        sectionViewFilters.forEach(sectionViewFilterItem => {
            const filterSettings = <IFilterSettings> {
                id: sectionViewFilterItem.queryFilterId,
                valueFieldName: sectionViewFilterItem.name,
                header: sectionViewFilterItem.hintText,
                inputType: sectionViewFilterItem.inputType,
                matchModeSelectorVisible: sectionViewFilterItem.userCanChangeOperator,
                autoSubmitSelectableInputChange: sectionViewFilterItem.autoSubmitInput
            };

            const filterType = sectionViewFilterItem.dataType ?? FilterType.Text;
            filterSettings.filterType = filterType;
            filterSettings.matchMode = this.getMatchModeByViewFilter(sectionViewFilterItem.queryRuleCode, filterType);

            result.push(filterSettings);
        });

        return result;
    }

    private createFooterOutput(values: {[key: string]: any[]}): {[key: string]: { value: string, textAlign: string}} {
        if (!values) return null;

        const result = {};

        const keys = Object.keys(values);
        keys.forEach(footerAggregatedValueKey => {
            const sectionViewColumnIndex = this._sectionView.columns
                .findIndex(sectionViewColumnItem => footerAggregatedValueKey ==
                    `${sectionViewColumnItem.dbDocColumnMetadata.staticData.parentTableName}_${sectionViewColumnItem.dbDocColumnMetadata.staticData.columnName}`);
            const sectionViewColumn = this._sectionView.columns[sectionViewColumnIndex];

            if (sectionViewColumnIndex > 0) {
                if (sectionViewColumn.footer.leftCellLabel) {
                    const prevSectionViewColumn = this._sectionView.columns[sectionViewColumnIndex - 1];
                    result[`${prevSectionViewColumn.dbDocColumnMetadata.staticData.parentTableName}_${prevSectionViewColumn.dbDocColumnMetadata.staticData.columnName}`]
                        = { value: sectionViewColumn.footer.leftCellLabel, textAlign: sectionViewColumn.footer.leftCellLabelAlignment };
                }
            }

            result[footerAggregatedValueKey] = {
                value: sectionViewColumn.footer.outputFormat
                    || values[footerAggregatedValueKey].map((x, index) => `{${index}}`).join(" / "),
                textAlign: sectionViewColumn.footer.textAlignment || "left"
            };
            values[footerAggregatedValueKey].forEach((aggregatedValue, index) =>
                result[footerAggregatedValueKey].value = (<string>result[footerAggregatedValueKey].value).replace(
                    `{${index}}`,
                    moment.isDate(aggregatedValue) ? moment(aggregatedValue).format("L") : aggregatedValue));
        });

        return result;
    }

    private createGridColumnSettings(columnViews: ISectionViewColumn[]): void {
        this._columnTemplates = [];
        this._tableSettings.columns = [];

        const priority1AutoWidthTypes = ["numeric", "bool"];
        const priority2AutoWidthTypes = ["date"];
        // If grid contains only priority 1 and priority 2 types columns then we do not set autowidth
        // For the priority 2 columns, otherwise none of column will get autowidth at all (they are dropped)
        const includePriority2Types = columnViews.some(x => x.visible
            && ![...priority1AutoWidthTypes, ...priority2AutoWidthTypes].includes(x.dbDocColumnMetadata.staticData.clrTypeGroup));

        columnViews.forEach(columnViewItem => {
            if (!columnViewItem.visible) return;

            const gridColumn = <IGridColumn>{
                field: `${columnViewItem.tableAlias ?? columnViewItem.dbDocColumnMetadata.staticData.parentTableName}_${columnViewItem.dbDocColumnMetadata.staticData.columnName}`,
                header: columnViewItem.inheritHeader ? columnViewItem.dbDocColumnMetadata.title : columnViewItem.header,
                displayMode: this.getDisplayModeByClrTypeGroup(columnViewItem.dbDocColumnMetadata.staticData.clrTypeGroup),
                sortable: columnViewItem.sortable,
                displayConditionalTrueValue: columnViewItem.extraSettings.trueValueLabel,
                displayConditionalFalseValue: columnViewItem.extraSettings.falseValueLabel,
                countNullAsFalse: columnViewItem.extraSettings.countNullAsFalse,
                displayDateMomentFormat: columnViewItem.extraSettings.dateMomentFormat,
                decimalPlaces: columnViewItem.extraSettings.decimalPlaces,
            };

            // TODO: this is temp. fix. MaxWidth setting is used to set Width. Then we should add a new setting for Width
            if (columnViewItem.extraSettings.maxWidth) {
                gridColumn.viewSettings = new GridColumnViewSettings({ width: columnViewItem.extraSettings.maxWidth + "px" });
            } else {
                const type = columnViewItem.dbDocColumnMetadata.staticData.clrTypeGroup;
                if (priority1AutoWidthTypes.includes(type)
                    || includePriority2Types && priority2AutoWidthTypes.includes(type)) {
                    gridColumn.viewSettings = new GridColumnViewSettings({ width: this.getColumnAutoWidthByClrTypeGroup(type) });
                }
            }

            let mask = columnViewItem.extraSettings.mask;
            if (columnViewItem.extraSettings.inheritMask) {
                mask = columnViewItem.dbDocColumnMetadata.viewMetadata.gridColumnView.mask;
            }
            if (columnViewItem.customColumnType) {
                mask = columnViewItem.customColumnType.viewMetadata?.gridColumnView?.mask;
            }
            if (mask) {
                gridColumn.displayHandler = this.getMaskDisplayHandler(mask, gridColumn);
            }

            if (columnViewItem.extraSettings.customFormat) {
                this._columnTemplates.push(<IColumnTemplate> {
                    column: gridColumn,
                    customTemplate: columnViewItem.extraSettings.customFormat
                });
            }

            this._tableSettings.columns.push(gridColumn);
        });
    }

    private getCustomFormattedCellHtml(customFormat: string, rowData: any): SafeHtml {
        let output = customFormat;

        const paramMatches = customFormat.match(/\{(.*?)\}/gm);
        if (paramMatches?.length) {
            paramMatches.forEach(matchItem => {
                const fieldName = matchItem.substring(1, matchItem.length - 1).replace(".", "_");
                const column = this._tableSettings.columns.find(x => x.field == fieldName);
                output = column ? output.replace(matchItem, GridHelper.getCellDisplayValue(rowData, column)) : "";
            });
        }

        return this.sanitizer.bypassSecurityTrustHtml(output);
    }

    private getColumnAutoWidthByClrTypeGroup(type: string) {
        switch (type) {
            case "date": return "180px";
            case "numeric":
            case "bool":
                return "90px";
            default:
                return "250px";
        }
    }

    private getDisplayModeByClrTypeGroup(clrTypeGroup: ClrTypeGroup): DisplayMode {
        switch (clrTypeGroup) {
            case "bool": return DisplayMode.Conditional;
            case "date": return DisplayMode.Date;
            case "numeric": return DisplayMode.Number;
            default: return DisplayMode.Text;
        }
    }

    private getFilterTypeByClrTypeGroup(clrTypeGroup: ClrTypeGroup): FilterType {
        switch (clrTypeGroup) {
            case "bool": return FilterType.Boolean;
            case "date": return FilterType.Date;
            case "numeric": return FilterType.Numeric;
            default: return FilterType.Text;
        }
    }

    private getMaskDisplayHandler(mask: string, column: IGridColumn): (cellValue: any, rowValue?: any) => string {
        return cellValue => {
            const cellValueString = GridHelper.convertCellRawValueToString(cellValue, column);
            let result = "";
            let cellDataIndex = 0;
            for (let maskIndex = 0; maskIndex < mask.length; maskIndex++) {
                if (mask[maskIndex] == "_") {
                    result += cellValueString[cellDataIndex++];
                } else {
                    result += mask[maskIndex];
                }
            }
            return result;
        }
    }

    private getMatchModeByViewFilter(
        queryRuleCode: QueryRuleCode,
        filterType: FilterType): CountableFilterMatchMode | StringFilterMatchMode {
        if (filterType === FilterType.Numeric || filterType === FilterType.Date || filterType === FilterType.Boolean) {
            switch (queryRuleCode) {
                case "notEquals": return CountableFilterMatchMode.NotEquals;
                case "less": return CountableFilterMatchMode.LessThan;
                case "lessOrEqual": return CountableFilterMatchMode.GreaterThanOrEqual;
                case "more": return CountableFilterMatchMode.GreaterThan;
                case "moreOrEqual": return CountableFilterMatchMode.GreaterThanOrEqual;
                case "between": return CountableFilterMatchMode.Between;
                default: return CountableFilterMatchMode.Equals
            }
        } else {
            switch (queryRuleCode) {
                case "equals": return StringFilterMatchMode.Equals;
                case "notEquals": return StringFilterMatchMode.NotEquals;
                case "startsWith": return StringFilterMatchMode.StartsWith;
                case "endsWith": return StringFilterMatchMode.EndsWith;
                case "notContains": return StringFilterMatchMode.NotContains;
                default: return StringFilterMatchMode.Contains;
            }
        }
    }

    loadFilterOptions(): void {
        if (!this._filter) return;

        this._sectionView.filters.forEach(filterControlItem => {
            if (filterControlItem.inputType === FilterInputType.Dropdown || filterControlItem.inputType === FilterInputType.Multiselect) {
                if (filterControlItem.extraSettings.sourceDbDocTableId &&
                    filterControlItem.extraSettings.labelDbDocColumnId &&
                    filterControlItem.extraSettings.valueDbDocColumnId) {
                    this.sectionService.getFilterOptions(this.sectionId, filterControlItem.filterControlId)
                        .then(result => this._filter.setOptions(filterControlItem.name, result));
                }
            }
        });
    }

    /** This section as master-section emits row select event to client-sections */
    _onGridRowSelect($event) {
        this.emitRowSelectedIfMasterSection($event.data);
    }

    _onGridRowUnselect($event) {
        this.emitRowSelectedIfMasterSection(null);
    }

    /** If this section as master is responsible for emitting row selecting event then do emit */
    emitRowSelectedIfMasterSection(data) {
        if (this._sectionView.masterSectionEmittedEvents.some(x => x === "rowSelected")) {
            this.emitMasterSectionGridRowSelectEvent(data);
        }
    }

    emitMasterSectionGridRowSelectEvent(eventData) {
        const sectionEvent = <IMasterSectionEvent>{
            eventType: "rowSelected",
            masterSectionId: this.sectionId,
            data: eventData
        };
        this.masterSectionCommand.emit(sectionEvent);
    }

    isSubscribedToMasterGridRowSelected(sectionView: ISectionDisplayView): boolean {
        return sectionView.masterSectionBindings.some(x => x.eventType == "rowSelected");
    }

    /** This section as client-section handles an event emitted by master-section */
    handleMasterSectionEvent(sectionEvent: IMasterSectionEvent) {
        switch (sectionEvent.eventType) {
            case "rowSelected":
                this.handleMasterSectionRowSelectedEvent(sectionEvent.masterSectionId, sectionEvent.data);
                break;
        }
    }

    emitSectionContainerStateChange(visible: boolean, expanded: boolean) {
        this.sectionContainerStateChange.emit({ sectionId: this.sectionId, visible: visible, expanded: expanded });
    }

    handleMasterSectionRowSelectedEvent(masterSectionId: string, rowData: any) {
        const binding = this._sectionView.masterSectionBindings.find(x => x.masterSectionId == masterSectionId
            && x.eventType == "rowSelected");

        if (binding) {
            //console.log("CLIENT SECTION HANDLER: ROW SELECTED", this.sectionId, rowData);

            if (rowData) { // If row selected
                this._masterSectionRowSelectEventBinding = binding;
                this._masterSectionRowSelectEventRowData = rowData;

                if (this._sectionContentVisible) {
                    this._grid.selection = [];
                    this._grid.reload();
                } else {
                    this._sectionContentVisible = true;
                }

                this.emitSectionContainerStateChange(true, true);
            } else { // If row unselected
                // Hiding this client-section's container
                this.emitSectionContainerStateChange(false, true);
            }

            // Emit row selected/unselected event to all client-sections.
            // It happens when it's a 3+ levels master-detail sections tree structure.
            this.emitRowSelectedIfMasterSection(null);
        }
    }
}