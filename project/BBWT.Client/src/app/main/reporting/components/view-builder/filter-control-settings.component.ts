import { Component, OnInit, ViewChild } from "@angular/core";
import { SafeHtml } from "@angular/platform-browser";

import { ConfirmationService, SelectItem } from "primeng/api";
import { Table } from "primeng/table";

import { IHash } from "@bbwt/interfaces";
import { FilterInputType, FilterType } from "@features/filter";
import { deepUpdate } from "@bbwt/utils";
import { ClrTypeGroup, IColumnMetadata, ITableMetadata } from "@main/dbdoc";
import {
    getQueryRuleCodeLabel,
    getQueryRuleOperator,
    IFilterControl,
    IQueryFilter,
    IQueryFilterBinding,
    ISqlFilterCodeInsert
} from "../../reporting-models";
import { SectionEditorComponent } from "../section-editor.component";
import { IViewBuilderController } from "../../interfaces/view-builder-controller";


@Component({
    selector: "filter-control-settings",
    templateUrl: "./filter-control-settings.component.html",
    styleUrls: ["./filter-control-settings.component.scss"]
})
export class FilterControlSettingsComponent implements OnInit {
    // TODO: to fix: on opening existing the DDL of control types should be filtered by linked query filter's type


    readonly _patternControlName: RegExp = /^[\w]*$/;
    readonly _tableColumns = [
        { field: "name", header: "Control Name" },
        { field: "inputType", header: "Control Type" },
        { field: "linkedQueryFilters", header: "Linked Query Filters" }
    ];

    _dataTypesOptions: SelectItem[];
    _displayEditControlDialog = false;
    _dropDownSourceTableColumnOptionsLoading: boolean;
    _editControlCreateFilter = false;
    _editControlFilterTableColumnId: string;
    _editControlFilterCondition: string;
    _editingControl: IFilterControl;
    _inputTypesOptions: SelectItem[];
    _loading: boolean;
    _linkedFiltersExpressionsMap: IHash<SafeHtml[]>;
    _queryRulesOptions: SelectItem[];
    _refreshing = false;
    _sourceTableColumnOptions: SelectItem[];
    _sourceTableOptions: ITableMetadata[];
    _tableColumnIdLinkedToFilterControl: string;
    _vbc: IViewBuilderController;

    @ViewChild("table", { static: false }) private _table: Table;


    constructor(public sectionEditorComponent: SectionEditorComponent,
                private confirmationService: ConfirmationService) {
        this._vbc = sectionEditorComponent;
    }


    get _filterInputTypeEnum(): any {
        return FilterInputType;
    }


    ngOnInit(): void {
        this.refresh();
    }


    refresh(): void {
        this._refreshLinkedFiltersExpressions();
    }

    _editingControlNameIsUnique(name: string): boolean {
        return this.sectionEditorComponent.section.view.filters
            .every(o => o.name != name || o.id == this._editingControl.id);
    }

    _filterDataTypeSelectionAvailable(): boolean {
        return !this._editControlFilterTableColumnId &&
            this._vbc.possibleFilterDataTypesOfInputTypesMap[this._editingControl.inputType] &&
            this._vbc.possibleFilterDataTypesOfInputTypesMap[this._editingControl.inputType].length > 1 &&
            (this._editingControl.inputType === FilterInputType.Number || this._editingControl.inputType === FilterInputType.Text) &&
            this._editingControl.queryFilterBindings.every(x => x.bindingType != "filterControl");
    }

    _getDefaultFilterDataTypeByInputType(inputType: FilterInputType): FilterType {
        switch (inputType) {
            case FilterInputType.Number: return FilterType.Numeric;
            case FilterInputType.Checkbox: return FilterType.Boolean;
            case FilterInputType.Calendar: return FilterType.Date;
            default: return FilterType.Text;
        }
    }

    _getFilterDataTypeByClrTypeGroup(clrTypeGroup: ClrTypeGroup): FilterType {
        switch (clrTypeGroup) {
            case "numeric": return FilterType.Numeric;
            default: return FilterType.Text;
        }
    }

    _onAddControlClick(): void {
        this._initFilterControlEditingDialog();
    }

    _onCreateFilterCheckboxChanged(): void {
        this._editControlFilterTableColumnId = null;
    }

    _onDeleteFilterControlClick(data: IFilterControl): void {
        this._loading = true;

        if (data.queryFilterBindings.length > 0) {
            this.confirmationService.confirm({
                message: "Also delete linked query filters?",
                accept: () => this._vbc.deleteFilterControl(data, true).finally(() => this._loading = false),
                reject: () => this._vbc.deleteFilterControl(data, false).finally(() => this._loading = false),
            });
        } else {
            this._vbc.deleteFilterControl(data, true).finally(() => this._loading = false);
        }
    }

    _onEditFilterControlClick(data: IFilterControl): void {
        this._initFilterControlEditingDialog(data);
    }

    async _onFilterTableColumnChanged(): Promise<void> {
        const metadata = this.sectionEditorComponent.queryColumnsMetadataMap[this._editControlFilterTableColumnId];
        if (metadata) {
            this._refreshing = true;

            const autoName = metadata.staticData.parentTableName + "_" + metadata.staticData.columnName;
            let uniqueName: string;
            let i = 0;
            do {
                uniqueName = autoName + (i == 0 ? "" : `_${i}`);
                i++;
            }
            while (!this._editingControlNameIsUnique(uniqueName));

            this._editingControl.name = uniqueName;
            this._editingControl.hintText = metadata.title;

            if (metadata.staticData.isForeignKey) {
                this._editingControl.inputType = FilterInputType.Dropdown;
                await this._preDefineSelectableSettingsForEditingFilterControl(metadata);
            } else {
                this._editingControl.inputType = this._vbc
                    .possibleFilterInputTypesOfClrTypesMap[metadata.staticData.clrTypeGroup][0].value;
                this._editingControl.dataType = this._getDefaultFilterDataTypeByInputType(this._editingControl.inputType);

                this._editingControl.extraSettings.sourceDbDocTableId = null;
                this._editingControl.extraSettings.labelDbDocColumnId = null;
                this._editingControl.extraSettings.valueDbDocColumnId = null;
            }
        }

        this._refreshQueryRulesOptions();
        this._refreshInputTypesOptions();
        this._refreshDataTypesOptions();
        this._refreshDropDownSourceTableColumnOptions();

        this._refreshing = false;
    }

    async _onInputTypeChanged(): Promise<void> {
        this._refreshDataTypesOptions();
        this._refreshDropDownSourceTableColumnOptions();

        this._editingControl.extraSettings.sourceDbDocTableId = null;
        this._editingControl.extraSettings.labelDbDocColumnId = null;
        this._editingControl.extraSettings.valueDbDocColumnId = null;

        if (!this._userCanChangeOperator(this._editingControl.inputType)) {
            this._editingControl.userCanChangeOperator = false;
        }

        const metadata = this.sectionEditorComponent.queryColumnsMetadataMap[this._editControlFilterTableColumnId];
        if (metadata?.staticData.isForeignKey) {
            await this._preDefineSelectableSettingsForEditingFilterControl(metadata);
        } else {
            this._editingControl.dataType = this._getDefaultFilterDataTypeByInputType(this._editingControl.inputType);
        }
    }

    async _onOptionsSourceTableChanged(): Promise<void> {
        this._dropDownSourceTableColumnOptionsLoading = true;

        let dbTable = this._vbc.tablesMetadata
            .find(o => o.tableId == this._editingControl.extraSettings.sourceDbDocTableId);

        await this._vbc.loadFullTableMetadata(dbTable.id);

        dbTable = this._vbc.tablesMetadata.find(x => x.id == dbTable.id);

        const pkColumn = dbTable.columns.find(x => x.staticData.isPrimaryKey)
            ?? dbTable.columns.find(x => x.staticData.columnName === "Id")
            ?? (dbTable.columns.length ? dbTable.columns[0] : null);

        this._editingControl.extraSettings.labelDbDocColumnId = pkColumn?.columnId;
        this._editingControl.extraSettings.valueDbDocColumnId = pkColumn?.columnId;
        this._refreshDropDownSourceTableColumnOptions(dbTable);

        this._dropDownSourceTableColumnOptionsLoading = false;
    }

    _onRowReordered($event: any): void {
        this._loading = true;
        this._vbc.moveFilterControl($event.dragIndex, $event.dropIndex).finally(() => this._loading = false);
    }

    _onSubmitFilterControlClick(): void {
        this._loading = true;

        this._setBindingToEditControl();

        if (!this._editingControl.id) {
            this._vbc.addFilterControl(this._editingControl)
                .then(() => this._displayEditControlDialog = false)
                .finally(() => this._loading = false);
        } else {
            this._vbc.updateFilterControl(this._editingControl)
                .then(() => this._displayEditControlDialog = false)
                .finally(() => this._loading = false);
        }
    }

    _userCanChangeOperator(type: FilterInputType): boolean {
        return type != FilterInputType.Checkbox &&
            type != FilterInputType.Dropdown &&
            type != FilterInputType.Multiselect;
    }


    private _initFilterControlEditingDialog(filterControl?: IFilterControl): void {
        this._resetNewControl();

        if (filterControl) {
            deepUpdate(this._editingControl, filterControl);
        }

        this._refreshLinkedToEditingFilterControlTableColumnId();
        this._refreshQueryRulesOptions();
        this._refreshInputTypesOptions();
        this._refreshDataTypesOptions();
        this._refreshDropDownSourceTableOptions();
        this._refreshDropDownSourceTableColumnOptions();

        this._editControlCreateFilter = false;

        this._displayEditControlDialog = true;
    }

    private async _preDefineSelectableSettingsForEditingFilterControl(columnMetadata: IColumnMetadata): Promise<void> {
        this._editingControl.dataType = this._getFilterDataTypeByClrTypeGroup(columnMetadata.staticData.clrTypeGroup);

        this._editingControl.extraSettings.sourceDbDocTableId =
            columnMetadata.staticData.tableReferences[0].targetTableId;

        const dbTable = this._vbc.tablesMetadata
            .find(o => o.tableId == this._editingControl.extraSettings.sourceDbDocTableId);

        await this._vbc.loadFullTableMetadata(dbTable.id);

        this._editingControl.extraSettings.labelDbDocColumnId =
            columnMetadata.staticData.tableReferences[0].targetColumnId;
        this._editingControl.extraSettings.valueDbDocColumnId =
            columnMetadata.staticData.tableReferences[0].targetColumnId;
    }

    private _queryFiltersLinkedToControlBySqlCode(filterControl: IFilterControl): IQueryFilter[]  {
        const queryFiltersMap = this.sectionEditorComponent.queryFiltersRelatedDataMap;

        if (!queryFiltersMap) return [];

        const filters: IQueryFilter[] = [];

        Object.keys(queryFiltersMap).forEach(x => {
            const queryFilter = queryFiltersMap[x].queryFilter;

            if (queryFilter.customSqlCodeInserts?.some(o => o.variableType == "filterControl" &&
                o.variableName == this._toSqlVariable(filterControl.name))) {
                filters.push(queryFilter);
            }
        });

        return filters;
    }

    private _refreshDataTypesOptions(): void {
        this._dataTypesOptions =
            this._vbc.possibleFilterDataTypesOfInputTypesMap[this._editingControl.inputType];
    }

    private _refreshDropDownSourceTableOptions(): void {
        this._sourceTableOptions = this._vbc.folders
            .find(x => x.id === this.sectionEditorComponent.section.query.dbDocFolderId)?.tables;

    }

    private _refreshDropDownSourceTableColumnOptions(sourceTableMetadata?: ITableMetadata): void {
        const dbTable = sourceTableMetadata
            ?? (this._editingControl.extraSettings.sourceDbDocTableId
                ? this._vbc.tablesMetadata
                    .find(o => o.tableId == this._editingControl.extraSettings.sourceDbDocTableId)
                : null);

        if (dbTable) {
            this._sourceTableColumnOptions = dbTable.columns.map(x =>
                <SelectItem>{ label: x.staticData.columnName, value: x.columnId });
        }
    }

    private _refreshInputTypesOptions(): void {
        const tableColumnId = this._tableColumnIdLinkedToFilterControl ?? this._editControlFilterTableColumnId;

        this._inputTypesOptions = this._vbc.possibleFilterInputTypesOfClrTypesMap[
            tableColumnId
                ? this.sectionEditorComponent.queryColumnsMetadataMap[tableColumnId].staticData.clrTypeGroup
                : "default"
            ];
    }

    private _refreshLinkedFiltersExpressions(): void {
        this._linkedFiltersExpressionsMap = {};

        const queryFiltersMap = this.sectionEditorComponent.queryFiltersRelatedDataMap;

        this.sectionEditorComponent.section.view?.filters.forEach(filterControl => {
            this._linkedFiltersExpressionsMap[filterControl.id] = [];

            // In bindings (Standard query filters)
            filterControl.queryFilterBindings.forEach(binding => {
                if (!binding?.queryFilterId) return null;

                const queryFilterRelatedData = queryFiltersMap[binding.queryFilterId];
                const queryFilterColumnStaticData = queryFilterRelatedData.columnMetadata.staticData;
                const ruleCode = queryFilterRelatedData.queryFilter.queryRule.code;
                const queryRuleCodeAsStr = (getQueryRuleOperator(ruleCode) || getQueryRuleCodeLabel(ruleCode));

                const operatorStr = queryRuleCodeAsStr.toLowerCase();
                const columnStr = this._toSqlVariable(
                    `${queryFilterColumnStaticData.parentTableName}.${queryFilterColumnStaticData.columnName}`);
                const filterControlStr = this._toSqlVariable(filterControl.name);

                this._linkedFiltersExpressionsMap[filterControl.id]
                    .push(`${this._toTableColumnHtml(columnStr)} ${operatorStr} ${this._toFilterControlHtml(filterControlStr)}`);
            });

            // In SQL code (SQL filters)
            this._queryFiltersLinkedToControlBySqlCode(this._editingControl).forEach(o => {
                this._linkedFiltersExpressionsMap[filterControl.id]
                    .push(this._sqlCodeToHtml(o.customSqlCodeTemplate, o.customSqlCodeInserts));
            });
        });
    }

    private _refreshLinkedToEditingFilterControlTableColumnId(): void {
        const queryFilterId = this._editingControl.queryFilterBindings
            .find(o => o.bindingType = "filterControl")?.queryFilterId;

        this._tableColumnIdLinkedToFilterControl = queryFilterId
            ? this.sectionEditorComponent.queryFiltersRelatedDataMap[queryFilterId].queryFilter.queryTableColumnId
            : null;
    }

    private _refreshQueryRulesOptions(): void {
        this._queryRulesOptions =
            this._vbc.possibleQueryRulesOfClrTypesMap[
                this.sectionEditorComponent.queryColumnsMetadataMap[this._editControlFilterTableColumnId]
                    ?.staticData.clrTypeGroup
            ];
    }

    private _resetNewControl(): void {
        this._editingControl = <IFilterControl> {
            inputType: FilterInputType.Text,
            extraSettings: {},
            queryFilterBindings: []
        };
        this._editControlFilterTableColumnId = null;
        this._tableColumnIdLinkedToFilterControl = null;
    }

    private _setBindingToEditControl() {
        if (this._editControlCreateFilter) {
            const binding = <IQueryFilterBinding>{
                bindingType: "filterControl",
                queryFilter: <IQueryFilter>{
                    queryTableColumnId: this._editControlFilterTableColumnId,
                    queryRuleId: this._editControlFilterCondition
                }
            };

            this._editingControl.queryFilterBindings = [binding];
        }
    }

    private _sqlCodeToHtml(sql: string, inserts: ISqlFilterCodeInsert[]): string {
        let shift = 0;
        let result = sql;

        inserts.forEach(insert => {
            const paste = insert.variableType == "filterControl" ?
                this._toFilterControlHtml(insert.variableName) :
                this._toTableColumnHtml(insert.variableName);

            result = result.slice(0, insert.position + shift) + paste +
                result.slice(insert.position + insert.variableName.length + shift);
            shift += paste.length - insert.variableName.length;
        });

        return result;
    }

    private _toSqlVariable(name: string) {
        return "@" + name;
    }

    private _toTableColumnHtml(variable: string) {
        return `<span class='query-expr-table-column'>${variable}</span>`;
    }

    private _toFilterControlHtml(variable: string) {
        return `<span class='query-expr-ui-control'>${variable}</span>`;
    }
}