import { Component, ElementRef, OnInit, QueryList, ViewChild, ViewChildren } from "@angular/core";

import { TieredMenu } from "primeng/tieredmenu";
import { SplitButton } from "primeng/splitbutton";
import { OverlayPanel } from "primeng/overlaypanel";
import { ConfirmationService, MenuItem, MessageService, SelectItem, TreeNode } from "primeng/api";

import { FilterInputType } from "@features/filter";
import { IHash } from "@bbwt/interfaces";
import { deepUpdate } from "@bbwt/utils";
import { Message } from "@bbwt/classes";
import { ClrTypeGroup } from "../../../dbdoc";
import {
    getFullColumnName,
    getQueryConditionalOperatorEnumAsOptions,
    ISaveMasterDetailBindingCommandEventData,
    IFilterControl,
    IQueryFilter,
    IQueryFilterSet, IQueryTableColumn,
    trueFalseOptions, IQueryFilterBinding
} from "../../reporting-models";
import { QueryBuilderComponent } from "./query-builder.component";
import { IQueryBuilderController } from "../../interfaces";


@Component({
    selector: "query-filters",
    templateUrl: "./query-filters.component.html",
    styleUrls: ["./query-filters.component.scss"]
})
export class QueryFiltersComponent implements OnInit {
    loading: boolean;

    _filterOptions: any;
    _sqlFilterOptions: any;

    readonly _calendarYearRange = `1900:${(new Date()).getFullYear()}`;
    _activeFilterForFilterOptions: IQueryFilter;
    _activeFilterForSqlFilterOptions: IQueryFilter;
    _autoSubmitFiltersMap: IHash<boolean> = {};
    _cbc: IQueryBuilderController;
    _conditionalOperatorOptions = getQueryConditionalOperatorEnumAsOptions();
    _filterInputTypeEnum = FilterInputType;
    _filterSetAddButtonModel: MenuItem[];
    _masterDetailFilterEditingDialogVisible = false;
    _masterDetailEditingData: ISaveMasterDetailBindingCommandEventData = {};
    _masterDetailSectionOptions: SelectItem[];
    _masterDetailColumnsOptions: SelectItem[];
    _nodes: TreeNode[] = [];
    _nodesCount: number;
    _originalFiltersMap: IHash<IQueryFilter> = {};
    _rowMenuModel: MenuItem[];
    _queryFiltersBindingEnabledMap: IHash<IFilterControl>;
    _sqlFilterOptionsVisible: boolean;
    _sqlFilterInputIdPrefix = "sqlfilter-";
    _trueFalseOptions = trueFalseOptions();

    @ViewChild("overlayPanelFilterOptions", { static: false }) private _overlayPanelFilterOptions: OverlayPanel;
    @ViewChild("overlayPanelSqlFilterOptions", { static: false }) private _overlayPanelSqlFilterOptions: OverlayPanel;
    @ViewChild("rowMenu", { static: false }) private _rowMenu: TieredMenu;
    @ViewChildren("sqlFilterInput") sqlFilterInputs: QueryList<ElementRef>;


    constructor(public queryBuilderComponent: QueryBuilderComponent,
                private confirmationService: ConfirmationService,
                private messageService: MessageService) {
        this._cbc = queryBuilderComponent.cbc;
    }


    ngOnInit(): void {
        this.refreshView();
    }


    refreshView(): void {
        this._refreshQueryFiltersBindingEnabledMap();
        this._buildNodes();

        this._bindSqlFilterOptions();
    }

    _getMasterDetailSourceQueryTableColumnName(queryTableColumnId: string): string {
        return getFullColumnName(this._cbc.queryColumnsMetadataMap[queryTableColumnId].staticData);
    }

    _getMasterDetailTargetSectionName(queryFilter: IQueryFilter): string {
        const masterDetailBinding = queryFilter.queryFilterBindings
            .find(x => x.bindingType === "masterDetailGrid");

        if (!masterDetailBinding?.masterDetailSectionId) return "undefined";

        const section = this.queryBuilderComponent.sectionEditorComponent?.reportEditorComponent.report.sections
            .find(x => x.id === masterDetailBinding.masterDetailSectionId);

        if (!section) return "undefined";

        return `'${section.title}'`;
    }

    _getMasterDetailTargetQueryTableColumnName(queryFilter: IQueryFilter): string {
        const masterDetailBinding = queryFilter.queryFilterBindings
            .find(x => x.bindingType === "masterDetailGrid");

        if (!masterDetailBinding?.masterDetailQueryTableColumn) return null;

        return getFullColumnName(this._cbc.columnsMetadata
            .find(x => x.columnId === masterDetailBinding.masterDetailQueryTableColumn.sourceColumnId)
            .staticData);
    }

    _onAddFilterSetClick(rowData: IQueryFilterSet): void {
        this.loading = true;
        this._cbc.addQueryFilterSet(rowData).finally(() => this.loading = false);
    }

    _onBetweenValueChanged(filter: IQueryFilter, betweenValue: any[]): void {
        filter.value = betweenValue[0];
        filter.value2 = betweenValue[1];
    }

    _onFilterSetAddButtonClicked(rowData: IQueryFilterSet, splitButton?: SplitButton, $event?: any): void {
        this._filterSetAddButtonModel  = [
            {
                label: "Filter (standard)",
                icon: "",
                command: () => {
                    this.loading = true;

                    const firstColumn = this.queryBuilderComponent.query.queryTables?.find(x => x.columns?.length)?.columns[0];
                    if (firstColumn) {
                        this._cbc.addQueryFilter(rowData, <IQueryFilter>{
                            queryTableColumnId: firstColumn?.id,
                            queryFilterSetId: rowData.id
                        }).finally(() => this.loading = false);
                    }
                }
            },
            {
                label: "SQL Filter",
                icon: "",
                command: () => {
                    this.loading = true;

                    this._cbc.addQueryFilter(rowData, <IQueryFilter> {
                        queryFilterSetId: rowData.id,
                        queryFilterSet: rowData
                    }).finally(() => this.loading = false);
                }
            },
            {
                label: "Master-detail Filter",
                icon: "",
                command: () => this._startMasterDetailEditing(<ISaveMasterDetailBindingCommandEventData> {
                    queryFilterSet: rowData,
                    queryTableColumnId: this.queryBuilderComponent.query.queryTables[0].columns
                        .find(x => this._cbc.queryColumnsMetadataMap[x.id]
                            .staticData.isForeignKey)?.id
                })
            }
        ];

        splitButton?.onDropdownButtonClick($event);
    }

    _onMasterDetailFilterEditingDialogHide(): void {
        this._masterDetailEditingData = {};
    }

    _onQueryFilterSetChanged(filterSet: IQueryFilterSet): void {
        this.loading = true;
        this._cbc.updateQueryFilterSet(filterSet)
            .finally(() => this.loading = false);
    }

    _onQueryFilterChangeSubmitted(filter: IQueryFilter): void {
        this.loading = true;
        this._cbc.updateQueryFilter(filter)
            .finally(() => this.loading = false);
    }

    _onQueryFilterValueChangeCancelled(filter: IQueryFilter): void {
        filter.value = this._originalFiltersMap[filter.id].value;
        filter.value2 = this._originalFiltersMap[filter.id].value2;
    }

    _onQueryFilterNumericRangeKeyDown($event: KeyboardEvent, filter: IQueryFilter): void {
        switch ($event.key) {
            case "Enter": this._onQueryFilterChangeSubmitted(filter); break;
            case "Escape": this._onQueryFilterValueChangeCancelled(filter); break;
        }
    }

    _onQueryFilterSetRowMenuClicked(rowData: IQueryFilterSet, $event: any): void {
        this._rowMenuModel = [
            {
                label: "Delete",
                icon: "pi pi-trash",
                command: () => {
                    if (rowData.queryFilters?.length || rowData.childSets?.length) {
                        this.confirmationService.confirm({
                            message: "Also you sure that you want to delete the Query Filter?",
                            accept: () => {
                                this.loading = true;
                                this._cbc.deleteQueryFilterSet(rowData)
                                    .finally(() => this.loading = false);
                            }
                        });
                    } else {
                        this.loading = true;
                        this._cbc.deleteQueryFilterSet(rowData)
                            .finally(() => this.loading = false);
                    }
                }
            }
        ];

        this._rowMenu.toggle($event);
    }

    _onQueryFilterRowMenuClicked(rowData: IQueryFilter, $event: any): void {
        if (rowData.queryFilterBindings.some(x => x.bindingType == "masterDetailGrid")) {
            this._rowMenuModel = [
                {
                    label: "Edit",
                    icon: "pi pi-pencil",
                    command: () => {
                        const masterDetailBinding = rowData.queryFilterBindings
                            .find(x => x.bindingType === "masterDetailGrid");

                        this._startMasterDetailEditing(<ISaveMasterDetailBindingCommandEventData> {
                            bindingId: masterDetailBinding.id,
                            queryTableColumnId: rowData.queryTableColumnId,
                            queryFilter: rowData,
                            masterSectionId: masterDetailBinding.masterDetailSectionId,
                            masterSectionTableColumnId: masterDetailBinding.masterDetailQueryTableColumnId
                        });
                    }
                },
                {
                    label: "Delete",
                    icon: "pi pi-trash",
                    command: () => {
                        this.loading = true;
                        this._cbc.deleteQueryFilter(rowData)
                            .finally(() => this.loading = false);
                    }
                }
            ]
        } else {
            this._rowMenuModel = [
                {
                    label: "Delete",
                    icon: "pi pi-trash",
                    command: () => {
                        this.loading = true;
                        this._cbc.deleteQueryFilter(rowData)
                            .finally(() => this.loading = false);
                    }
                }
            ];
        }

        this._rowMenu.toggle($event);
    }

    _onQueryTableColumnChanged(filter: IQueryFilter): void {
        if (filter.queryRuleId) {
            if (this._cbc.possibleQueryRulesOfClrTypesMap[
                this._cbc.queryColumnsMetadataMap[filter.queryTableColumnId].staticData.clrTypeGroup]
                .every(x => x.value != filter.queryRuleId)) {
                const possibleRules = this._cbc.possibleQueryRulesOfClrTypesMap[
                    this._cbc.queryColumnsMetadataMap[filter.queryTableColumnId].staticData.clrTypeGroup];
                filter.queryRuleId = possibleRules?.length ? possibleRules[0].value : null;
            }
        }

        filter.value = null;
        filter.value2 = null;

        this._onQueryFilterChangeSubmitted(filter);

        // TODO: if query filter is already linked to UI control via bindings and an input type of the linked control doesn't
        // Match the list of input types allowed for the clr type of the new selected table column, then we either:
        // 1) auto-remove binding to linked UI control
        // 2) keep the binding but warn user that there is types mismatch
        // 3) do not allow to change to this table column
    }

    _onFilterOptionsToggle(filter: IQueryFilter, e: any): void {
        if (this._overlayPanelFilterOptions.overlayVisible) {
            this._overlayPanelFilterOptions.hide();
        } else {
            this._showFilterOptionsPanel(filter, e);
        }
    }

    _onFilterBindingOptionSelected(e: any): void {
        this.loading = true;
        this._cbc.bindFilterControlToQueryFilter(e.value, this._activeFilterForFilterOptions)
            .finally(() => this.loading = false);

        this._overlayPanelFilterOptions.hide();
    }

    _onFilterDeleteBindingClicked(queryFilter: IQueryFilter) {
        this.loading = true;

        const binding = this._cbc.queryFiltersRelatedDataMap[queryFilter.id]
            .filterControl?.queryFilterBindings
            .find(x => x.bindingType === "filterControl" && x.queryFilterId === queryFilter.id);
        if (binding) {
            this._cbc.deleteQueryFilterBinding(binding)
                .finally(() => this.loading = false);
        }
    }

    async _onMasterSectionChanged(): Promise<void> {
        this._masterDetailEditingData.masterSectionTableColumnId = null;
        await this._refreshMasterDetailColumnsOptions();
    }

    _onSqlFilterKeydown(filter: IQueryFilter, e: any): void {
        if (this._overlayPanelSqlFilterOptions.overlayVisible) {
            if (e.key != "@") {
                this._overlayPanelSqlFilterOptions.hide();
            }
        } else {
            if (e.key == "@") {
                this._showSqlFilterOptionsPanel(filter, e);
            } else if (e.key == "Enter") {
                this._onSqlFilterChangeSubmitted(filter);
            }
        }
    }

    _onSqlFilterChangeSubmitted(filter: IQueryFilter): void {
        if (filter.customSqlCodeTemplate !== this._originalFiltersMap[filter.id].customSqlCodeTemplate) {
            this.loading = true;
            this._cbc.updateQueryFilter(filter).finally(() => this.loading = false);
        }
    }

    _onSqlFilterChangeCancelled(filter: IQueryFilter): void {
        filter.customSqlCodeTemplate = this._originalFiltersMap[filter.id].customSqlCodeTemplate;
    }

    _onSqlFilterOptionsToggle(filter: IQueryFilter, e: any): void {
        if (this._overlayPanelSqlFilterOptions.overlayVisible) {
            this._overlayPanelSqlFilterOptions.hide();
        } else {
            this._showSqlFilterOptionsPanel(filter, e);
        }
    }

    _onSqlFilterPasteOptionSelected(e: any): void {
        const filter = this._activeFilterForSqlFilterOptions;

        const option = this._cbc.queryColumnsOptions.find(x => x.value === e.value)
            ?? this._cbc.filterControlsOptions.find(x => x.value === e.value);

        const sqlFilterInput = this._getSqlFilterInputByFilterId(filter.id);
        if (option) {
            let pasteValue = option.label;
            const caretPos = this.getElementCaretPos(sqlFilterInput);
            if (filter.customSqlCodeTemplate.length === 0 || filter.customSqlCodeTemplate[caretPos - 1] !== "@") {
                pasteValue = "@" + pasteValue;
            }

            let resultStr = filter.customSqlCodeTemplate;
            resultStr = resultStr.slice(0, caretPos) + pasteValue + resultStr.slice(caretPos);
            filter.customSqlCodeTemplate = resultStr;

            this._overlayPanelSqlFilterOptions.hide();
            sqlFilterInput.nativeElement.focus();

            const newCaretPos = caretPos + pasteValue.length;
            setTimeout(() => {
                sqlFilterInput.nativeElement.setSelectionRange(newCaretPos, newCaretPos);
            }, 10);
        } else {
            this._overlayPanelSqlFilterOptions.hide();
            sqlFilterInput.nativeElement.focus();
        }
    }

    _calcNodeFilterIndentStyle(level: number): object {
        return {
            "margin-left": (level-1) * 16 + "px"
        };
    }

    async _refreshMasterDetailColumnsOptions(): Promise<void> {
        const section = this.queryBuilderComponent.sectionEditorComponent.reportEditorComponent
            .report.sections.find(x => x.id === this._masterDetailEditingData.masterSectionId);

        if (!section) return;

        await this.queryBuilderComponent.sectionEditorComponent.reportEditorComponent.loadFullSection(section);

        this._masterDetailColumnsOptions = section.query.queryTables.reduce((a, c) =>
            a.concat(c.columns
                .sort((a, b) => {
                    const aStaticData = this._cbc.columnsMetadata
                        .find(x => x.columnId === a.sourceColumnId).staticData;
                    const bStaticData = this._cbc.columnsMetadata
                        .find(x => x.columnId === b.sourceColumnId).staticData;

                    return aStaticData.isPrimaryKey && !bStaticData.isPrimaryKey
                        ? -1
                        : !aStaticData.isPrimaryKey && bStaticData.isPrimaryKey ? 1 : 0;
                })
                .map((x: IQueryTableColumn) => <SelectItem> {
                    label: `${getFullColumnName(this._cbc.columnsMetadata
                        .find(y => y.columnId === x.sourceColumnId).staticData)}`,
                    value: x.id
                })
            ), []);
    }

    _saveMasterDetailFilter(): void {
        this.loading = true;
        if (!this._masterDetailEditingData.bindingId) {
            this._cbc.addQueryFilter(
                this._masterDetailEditingData.queryFilterSet,
                <IQueryFilter> {
                    queryTableColumnId: this._masterDetailEditingData.queryTableColumnId,
                    queryFilterSetId: this._masterDetailEditingData.queryFilterSet.id,
                    queryFilterSet: this._masterDetailEditingData.queryFilterSet,
                    queryFilterBindings: [
                        <IQueryFilterBinding> {
                            bindingType: "masterDetailGrid",
                            masterDetailSectionId: this._masterDetailEditingData.masterSectionId,
                            masterDetailQueryTableColumnId: this._masterDetailEditingData.masterSectionTableColumnId
                        }
                    ]
                }).finally(() => this.loading = false);
        } else {
            const binding = <IQueryFilterBinding> {
                id: this._masterDetailEditingData.bindingId,
                bindingType: "masterDetailGrid",
                queryFilter: this._masterDetailEditingData.queryFilter,
                queryFilterId: this._masterDetailEditingData.queryFilter.id,
                masterDetailSectionId: this._masterDetailEditingData.masterSectionId,
                masterDetailQueryTableColumnId: this._masterDetailEditingData.masterSectionTableColumnId
            };
            binding.queryFilter.queryTableColumnId = this._masterDetailEditingData.queryTableColumnId;

            this._cbc.updateMasterDetailFilterBinding(binding)
                .finally(() => this.loading = false);
        }

        this._masterDetailFilterEditingDialogVisible = false;
    }

    async _startMasterDetailEditing(masterDetailEventData?: ISaveMasterDetailBindingCommandEventData): Promise<void> {
        if (!this.queryBuilderComponent.sectionEditorComponent) return;

        if (this.queryBuilderComponent.sectionEditorComponent.reportEditorComponent.report.sections.length <= 1) {
            this.messageService.add(Message.Warning(
                "There is no other section for master detail filter binding.",
                "Impossible to create a master detail filter"));
            return;
        }

        this._masterDetailEditingData = masterDetailEventData;

        this._masterDetailSectionOptions = this.queryBuilderComponent.sectionEditorComponent.reportEditorComponent
            .report.sections
            .filter(x => x.id !== this.queryBuilderComponent.sectionEditorComponent.section.id)
            .map(x => <SelectItem> { label: x.title, value: x.id });

        if (masterDetailEventData.masterSectionId) {
            await this._refreshMasterDetailColumnsOptions();
        } else {
            this._masterDetailColumnsOptions = null;
        }

        this._masterDetailFilterEditingDialogVisible = true;
    }

    private _getLinkedFilterControlHtmlName(filter: IQueryFilter): string {
        const controlName = this._cbc.queryFiltersRelatedDataMap[filter.id].filterControl?.name;
        return this._toFilterControlHtml(this._toSqlVariable(controlName));
    }

    private _toSqlVariable(name: string) {
        return "@" + name;
    }

    private _toFilterControlHtml(variable: string) {
        return `<span class='query-expr-ui-control'>${variable}</span>`;
    }

    private _showFilterOptionsPanel(filter: IQueryFilter, e) {
        this._bindFilterOptions(filter);
        this._overlayPanelFilterOptions.show(e);
        this._activeFilterForFilterOptions = filter;
    }

    // Binding @ options for standard query filter
    private _bindFilterOptions(filter: IQueryFilter) {
        this._filterOptions = [];
        const possibleOptions = this._cbc.queryFiltersRelatedDataMap[filter.id].possibleFilterControls;
        if (possibleOptions?.length) {
            this._filterOptions.push({
                label: "UI Controls",
                value: "control",
                items: possibleOptions
            });
        }
    }

    // Binding @ options for SQL filter
    private _bindSqlFilterOptions() {
        this._sqlFilterOptions = [];

        if (this._cbc.queryColumnsOptions?.length) {
            this._sqlFilterOptions.push({
                label: "Table Columns",
                value: "column",
                items: this._cbc.queryColumnsOptions
            });
        }

        if (this._cbc.filterControlsOptions?.length) {
            this._sqlFilterOptions.push({
                label: "UI Controls",
                value: "control",
                items: this._cbc.filterControlsOptions
            });
        }
    }

    private _getSqlFilterInputByFilterId(filterId: string): ElementRef {
        return this.sqlFilterInputs.find(o => o.nativeElement.id == this._sqlFilterInputIdPrefix + filterId);
    }

    private _showSqlFilterOptionsPanel(filter: IQueryFilter, e) {
        this._overlayPanelSqlFilterOptions.show(e);
        this._activeFilterForSqlFilterOptions = filter;
    }

    private _addFilterNode(filter: IQueryFilter, parentNodesCollection: TreeNode[]): void {
        if (filter.queryFilterBindings.some(x => x.bindingType == "masterDetailGrid")) {
            const node = {
                data: filter,
                type: "masterDetailFilter"
            } as TreeNode;
            parentNodesCollection.push(node);

            this._autoSubmitFiltersMap[filter.id] = true;
        } else if (filter.queryTableColumnId) {
            // Standard filter
            const columnStaticData = this._cbc.queryFiltersRelatedDataMap[filter.id].columnMetadata.staticData;
            const inputType = this._getFilterInputTypeByClrType(columnStaticData.clrTypeGroup); 
            const node = {
                data: filter,
                type: "queryFilter",
                inputType: inputType
            } as TreeNode;
            parentNodesCollection.push(node);

            this._autoSubmitFiltersMap[filter.id] =
                inputType == FilterInputType.Checkbox || 
                inputType == FilterInputType.Calendar &&
                    !this._cbc.queryFiltersRelatedDataMap[filter.id].isBetween ||
                inputType == FilterInputType.Dropdown;
        } else {
            // SQL filter
            const node = {
                data: filter,
                type: "sqlFilter"
            } as TreeNode;
            parentNodesCollection.push(node);

            this._autoSubmitFiltersMap[filter.id] = false;
        }

        // Store original filters content in map
        this._originalFiltersMap[filter.id] = <IQueryFilter>{};
        deepUpdate(this._originalFiltersMap[filter.id], filter);
        this._originalFiltersMap[filter.id].value = filter.value;
        this._originalFiltersMap[filter.id].value2 = filter.value2;

        this._nodesCount++;
    }

    private _addFilterSetNode(filterSet: IQueryFilterSet, parentNodesCollection: TreeNode[]): void {
        const node = {
            data: filterSet,
            expanded: true,
            type: "queryFilterSet",
            children: []
        } as TreeNode;
        parentNodesCollection.push(node);

        filterSet.queryFilters.forEach(x => this._addFilterNode(x, node.children));
        filterSet.childSets.forEach(x => this._addFilterSetNode(x, node.children));

        this._nodesCount++;
    }

    private _buildNodes(): void {
        if (!this.queryBuilderComponent.query?.rootFilterSet) return;

        const nodes = [];
        this._nodesCount = 0;
        this._addFilterSetNode(this.queryBuilderComponent.query.rootFilterSet, nodes);
        this._nodes = nodes;
    }

    private _getFilterInputTypeByClrType(columnClrType: ClrTypeGroup): FilterInputType {
        switch (columnClrType) {
            case "date": return FilterInputType.Calendar;
            case "numeric": return FilterInputType.Number;
            case "bool": return FilterInputType.Checkbox;
            default: return FilterInputType.Text;
        }
    }

    private _refreshQueryFiltersBindingEnabledMap(): void {
        this._queryFiltersBindingEnabledMap = {};

        if (this._cbc.queryFiltersRelatedDataMap) {
            Object.keys(this._cbc.queryFiltersRelatedDataMap).forEach(x =>
                this._queryFiltersBindingEnabledMap[
                    this._cbc.queryFiltersRelatedDataMap[x].queryFilter.id] =
                    this._cbc.queryFiltersRelatedDataMap[x].filterControl);
        }
    }

    private getElementCaretPos(elRef: ElementRef) {
        const el = elRef.nativeElement;
        return el.selectionDirection == "backward" ? el.selectionStart : el.selectionEnd;
    }
}