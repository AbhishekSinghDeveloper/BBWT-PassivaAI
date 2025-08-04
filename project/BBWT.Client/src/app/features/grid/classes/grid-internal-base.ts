import { ChangeDetectorRef, Directive, ViewChild } from "@angular/core";
import { Validators } from "@angular/forms";

import { Table } from "primeng/table";
import { FilterMatchMode, FilterMetadata, LazyLoadEvent, SelectItem } from "primeng/api";

import { FilterComponent, FilterInputType, FilterType } from "@features/filter";
import { notEmptyValidator } from "@bbwt/modules/validation";
import { GridCrudManager } from "./grid-crud-manager";
import { GridDataEditor } from "./grid-data-editor";
import { GridDataViewer } from "./grid-data-viewer";
import { GridQueryManager } from "./grid-query-manager";
import { GridExporter } from "./grid-exporter";
import { GridImporter } from "./grid-importer";
import { CreateMode } from "../enums/create-mode";
import { UpdateMode } from "../enums/update-mode";
import { IGridSettings } from "../interfaces/grid-settings";
import { ITableSettings } from "../interfaces/table-settings";
import { IGridColumn } from "../interfaces/grid-column";
import { IGridExternalMetadata } from "../interfaces/grid-external-metadata";
import { GridValidator } from "../classes/grid-validator";


@Directive()
export abstract class GridInternalBase {
    readonly _defaultCalendarYearRange = `1900:${new Date().getFullYear()}`;

    _ableToBeVisibleColumns: IGridColumn[];
    _crudManager: GridCrudManager;
    _dataEditor: GridDataEditor;
    _dataViewer: GridDataViewer;
    _exporter: GridExporter;
    _externalMetadata: IGridExternalMetadata;
    _filterComponent: FilterComponent;
    _gridSettings = {
        cancelButtonText: "Cancel",
        createMode: CreateMode.Dialog,
        createButtonText: "Add",
        dataKeyGenerator: () => {
            if (this._table.dataKey) {
                if (this._table.value?.length) {
                    const recordsKeys = this._table.value.map(x => Number(x[this._table.dataKey]));
                    if (recordsKeys.some(x => isNaN(x))) {
                        throw new Error("Grid creation error: Impossible to resolve the next data key for a new local created record.");
                    } else {
                        return Math.max(...recordsKeys) + 1;
                    }
                } else {
                    return 1;
                }
            }

            throw new Error("Grid creation error: Impossible to resolve the next data key for a new local created record.");
        },
        dataServiceCreateMethodName: "create",
        dataServiceDeleteMethodName: "delete",
        dataServiceDeleteAllMethodName: "deleteAll",
        dataServiceGetPageMethodName: "getPage",
        dataServiceUpdateMethodName: "update",
        dialogCancelButtonText: "Cancel",
        dialogSaveButtonText: "Save",
        importAllowedFormats: ".csv",
        updateMode: UpdateMode.Dialog,
        saveButtonText: "Save",
        sortEnabled: true
    } as IGridSettings;
    _importer: GridImporter;
    _printing = false;
    _queryManager: GridQueryManager;
    _selectedVisibleColumns: string[];
    @ViewChild(Table, { static: true }) _table: Table;
    _tableSettings = {
        dataKey: "id",
        editMode: "row",
        lazy: true,
        paginator: true,
        responsive: true,
        rows: 10,
        rowsPerPageOptions: [5, 10, 20],
    } as ITableSettings;
    _viewInitialized = false;


    protected constructor(public _cd: ChangeDetectorRef) {}


    _getColumnStyles(column: IGridColumn): {[key: string]: string} {
        const columnViewSettings = column.viewSettings ? column.viewSettings.getStyles() : {};
        const externalColumnMetadataKey = this._externalMetadata
            ? Object.keys(this._externalMetadata).find(x => x.toLowerCase() == column.field.toLowerCase())
            : null;
        const externalColumnViewSettings = externalColumnMetadataKey
            ? this._externalMetadata[externalColumnMetadataKey]?.viewSettings?.getStyles()
            : null;

        return externalColumnViewSettings
            ? Object.assign(columnViewSettings, externalColumnViewSettings)
            : columnViewSettings;
    }

    _getColumnValidators(column: IGridColumn): GridValidator[] {
        const columnValidators = column.validators || [];
        const externalColumnMetadataKey = this._externalMetadata
            ? Object.keys(this._externalMetadata).find(x => x.toLowerCase() == column.field.toLowerCase())
            : null;
        const externalColumnValidators = externalColumnMetadataKey
            ? this._externalMetadata[externalColumnMetadataKey]?.validators
            : null;

        const result = columnValidators;
        if (Array.isArray(externalColumnValidators)) {
            externalColumnValidators.forEach(externalColumnValidator => {
                const existingValidatorIndex = result.findIndex(x => x.errorKey == externalColumnValidator.errorKey);
                if (existingValidatorIndex >= 0) {
                    result.splice(existingValidatorIndex, 1, externalColumnValidator);
                } else {
                    result.push(externalColumnValidator);
                }
            });
        }
        return result;
    }

    _getColumnsCount(): number {
        let result = !this._table?.columns ? 0 : this._table.columns.filter(x => this._isColumnVisible(x)).length;
        if (this._showActionsColumn()) result++;
        if (this._table.selectionMode && this._gridSettings.selectColumn) result++;
        if (this._gridSettings.reorderableRows) result++;
        if (this._gridSettings.rowExpansionEnabled) result++;

        return result;
    }

    _getFirstFilterMetadata(filter: FilterMetadata | FilterMetadata[]): FilterMetadata {
        return Array.isArray(filter) ? (<FilterMetadata[]> filter)[0] : <FilterMetadata> filter;
    }

    _isColumnVisible(column: IGridColumn): boolean {
        return column.visible !== false &&
            (!this._gridSettings.visibleColumnsSelector || this._selectedVisibleColumns?.some(x => x == column.field))
    }

    _isRequiredColumn(column: IGridColumn): boolean {
        return (this._getColumnValidators(column) || [])
            .some(item => item.validator == Validators.required || item.validator == notEmptyValidator);
    }

    _onCreationButtonClick(): void {
        this._dataEditor.startCreation();
    }

    _onDeleteAllButtonClick(): void {
        this._dataEditor.startDeletingAll();
    }

    _onDeleteButtonClick(rowData: any, rowIndex?: number): void {
        this._dataEditor.startDeleting(rowData, rowIndex);
    }

    _onEditingButtonClick(rowData: any, rowIndex?: number): void {
        this._dataEditor.startUpdating(rowData, rowIndex);
    }

    _onImportDialogShowButtonClick(): void {
        this._importer.importDialogVisible = true;
    }

    _onInlineEditingButtonClick(rowData: any, rowIndex?: number): void {
        Object.keys(this._table.editingRowKeys).forEach(x => {
            if (x != rowData[this._table.dataKey]) {
                delete this._table.editingRowKeys[x];
            }
        });
        this._dataEditor.startUpdating(rowData, rowIndex);
    }

    _onInlineSaveButtonClick(): void {
        if (this._dataEditor.formGroup.valid) {
            this._dataEditor.saveEditing();
        }
    }

    _onInlineCancelButtonClick(): void {
        this._dataEditor.cancelEditing();
    }

    _onLazyLoad(lazyLoadEvent: LazyLoadEvent): void {
        this._queryManager.checkLazyLoadEvent(lazyLoadEvent);

        if (this._viewInitialized) {
            this._crudManager.loadData(lazyLoadEvent);
        }
    }

    _onOutputCellValueChanged(rowValue: any, column: IGridColumn): void {
        if (column.outputValueChangedHandler) {
            column.outputValueChangedHandler(rowValue);
        }
    }

    _showActionsColumn(): boolean {
        return (!this._gridSettings.readonly &&
            (this._gridSettings.updateMode != UpdateMode.Disabled ||
                this._gridSettings.deletingEnabled !== false) ||
            this._gridSettings.additionalRowActions?.length)
            && !this._printing;
    }

    _showButtonsBottomPanel(): boolean {
        return (this._gridSettings.createMode != CreateMode.Disabled
            || this._gridSettings.additionalActions?.length)
            && !this._printing;
    }

    _rowDeleteButtonVisible(rowData: any, rowIndex?: number): boolean {
        if (!rowData) return false;

        return !this._gridSettings?.readonly &&
            this._gridSettings.deletingEnabled !== false &&
            (!this._gridSettings.deletingAvailable || this._gridSettings.deletingAvailable(rowData)) &&
            (this._gridSettings.updateMode != UpdateMode.Inline || !this._dataEditor.updatingState ||
                this._table.dataKey && rowData[this._table.dataKey] != this._dataEditor.editableRow[this._table.dataKey] ||
                !this._table.dataKey && rowIndex != this._dataEditor.editableRowIndex);
    }


    protected _checkConfigurationValidity(): void {
        if (this._gridSettings.dataService != null) {
            if (!this._gridSettings.dataServiceGetPageMethodName) {
                this._throwGridConfigurationError("The 'dataServiceGetPageMethodName' is required when the 'dataService' specified.");
            } else if (!this._gridSettings.dataService[this._gridSettings.dataServiceGetPageMethodName]) {
                this._throwGridConfigurationError("The service specified in the 'dataService' parameter does not contains the method specified in the 'dataServiceGetPageMethodName' parameter.");
            }
        }

        if (!this._gridSettings.readonly) {
            if (this._gridSettings.dataService != null) {
                if (!this._table.dataKey) {
                    this._throwTableConfigurationError("The 'dataKey' is required when the 'dataService' specified.");
                }
            }

            switch (this._gridSettings.createMode) {
                case CreateMode.Redirect:
                    if (!this._gridSettings.createLink) {
                        this._throwGridConfigurationError("The 'createLink' is required when the 'createMode' is 'Redirect'.");
                    }
                    break;
                case CreateMode.External:
                    if (!this._gridSettings.createFunc) {
                        this._throwGridConfigurationError("The 'createFunc' is required when the 'createMode' is 'External'.");
                    }
                    break;
                case CreateMode.Dialog:
                    if (this._gridSettings.dataService != null) {
                        if (!this._gridSettings.dataServiceCreateMethodName) {
                            this._throwGridConfigurationError("The 'dataServiceCreateMethodName' is required when the 'dataService' specified.");
                        } else if (!this._gridSettings.dataService[this._gridSettings.dataServiceCreateMethodName]) {
                            this._throwGridConfigurationError("The service specified in the 'dataService' parameter does not contains the method specified in the 'dataServiceCreateMethodName' parameter.");
                        }
                    }
                    break;
            }

            switch (this._gridSettings.updateMode) {
                case UpdateMode.Redirect:
                    if (!this._gridSettings.updateLink) {
                        this._throwGridConfigurationError("The 'editingLink' is required when the 'updateMode' is 'Redirect'.");
                    }
                    const updateLinkParamMatches = this._gridSettings.updateLink.match(/:\w+/);
                    if (!updateLinkParamMatches) {
                        this._throwGridConfigurationError("The 'editingLink' must contain a parameter started from the colon char. Ex.: 'some-url/:id'");
                    }
                    if (updateLinkParamMatches.length > 1) {
                        this._throwGridConfigurationError("The 'editingLink' must contain only one parameter started from the colon char. Multiple parameters URL is not yet supported.");
                    }
                    break;
                case UpdateMode.External:
                    if (!this._gridSettings.updateFunc) {
                        this._throwGridConfigurationError("The 'editingFunc' is required when the 'updateMode' is 'External'.");
                    }
                    break;
                case UpdateMode.Dialog:
                    if (this._gridSettings.dataService != null) {
                        if (!this._gridSettings.dataServiceUpdateMethodName) {
                            this._throwGridConfigurationError("The 'dataServiceUpdateMethodName' is required when the 'dataService' specified.");
                        } else if (!this._gridSettings.dataService[this._gridSettings.dataServiceUpdateMethodName]) {
                            this._throwGridConfigurationError("The service specified in the 'dataService' parameter does not contains the method specified in the 'dataServiceUpdateMethodName' parameter.");
                        }
                    }
                    break;
                case UpdateMode.Inline:
                    if (this._gridSettings.dataService != null) {
                        if (!this._gridSettings.dataServiceUpdateMethodName) {
                            this._throwGridConfigurationError("The 'dataServiceUpdateMethodName' is required when the 'dataService' specified.");
                        } else if (!this._gridSettings.dataService[this._gridSettings.dataServiceUpdateMethodName]) {
                            this._throwGridConfigurationError("The service specified in the 'dataService' parameter does not contains the method specified in the 'dataServiceUpdateMethodName' parameter.");
                        }
                    } else {
                        if (!this._table.dataKey) {
                            this._throwTableConfigurationError("The 'dataKey' is required when the 'Inline' updating mode is chosen.");
                        }
                    }
                    break;
            }

            if (this._gridSettings.deletingEnabled !== false && !this._gridSettings.deleteFunc && this._gridSettings.dataService != null) {
                if (!this._gridSettings.dataServiceDeleteMethodName) {
                    this._throwGridConfigurationError("The 'dataServiceDeleteMethodName' is required when the 'dataService' specified.");
                } else if (!this._gridSettings.dataService[this._gridSettings.dataServiceDeleteMethodName]) {
                    this._throwGridConfigurationError("The service specified in the 'dataService' parameter does not contains the method specified in the 'dataServiceDeleteMethodName' parameter.");
                }
            }
        }

        if (this._gridSettings.deletingEnabled !== false && this._gridSettings.deleteAllEnabled && !this._gridSettings.deleteAllFunc && this._gridSettings.dataService != null) {
            if (!this._gridSettings.dataServiceDeleteAllMethodName) {
                this._throwGridConfigurationError("The 'dataServiceDeleteAllMethodName' is required when the 'dataService' specified.");
            } else if (!this._gridSettings.dataService[this._gridSettings.dataServiceDeleteAllMethodName]) {
                this._throwGridConfigurationError("The service specified in the 'dataService' parameter does not contains the method specified in the 'dataServiceDeleteAllMethodName' parameter.");
            }
        }
    }

    protected async _initGridSettings(): Promise<void> {
        if (this._exporter) {
            this._exporter.refresh();
        }

        if (this._gridSettings.externalMetadata) {
            const externalMetadata = this._gridSettings.externalMetadata();
            if (externalMetadata["then"] != null) {
                this._externalMetadata = await (externalMetadata as Promise<IGridExternalMetadata>);
            } else {
                this._externalMetadata = (externalMetadata as IGridExternalMetadata);
            }
        }
    }

    protected _initVisibleColumnsSelectorOptions(): void {
        if (!this._gridSettings.visibleColumnsSelector) return;

        this._ableToBeVisibleColumns = this._tableSettings.columns.filter(x => x.visible !== false);
        this._selectedVisibleColumns = this._ableToBeVisibleColumns.map(x => x.field);
    }

    protected _setTableSettings(): void {
        if (!this._viewInitialized || !this._tableSettings) return;

        Object.keys(this._tableSettings).forEach(tableSettingsKey =>
            this._setTableProperty(tableSettingsKey, this._tableSettings[tableSettingsKey]));

        if (!this._table.lazy && this._table.value && this._tableSettings.totalRecords == null) {
            this._table.totalRecords = this._table.value.length;
        }

        this._initVisibleColumnsSelectorOptions();
        this._tryRestoreState();

        // This action will perform the initial search and data loading if a FilterComponent exist.
        this._queryManager.initFilterComponent();

        // Perform the initial data loading manually if a FilterComponent doesn't exist.
        if (!this._filterComponent) {
            this._crudManager.loadData(this._queryManager.getLazyLoadMetadata());
        }
    }

    protected _setTableProperty(propertyName: string, data: any): void {
        if (!this._viewInitialized) {
            this._tableSettings[propertyName] = data;
            return;
        }

        this._table[propertyName] = data;

        if (propertyName == "value") {
            this._table.totalRecords = this._table.value.length;
        }

        if (propertyName == "columns") {
            this._initVisibleColumnsSelectorOptions();
        }
    }

    protected _throwGridConfigurationError(message: string): void {
        throw new Error(`Grid configuration error: ${message}`);
    }

    protected _throwTableConfigurationError(message: string): void {
        throw new Error(`Table configuration error: ${message}`);
    }

    protected _tryRestoreState(): void {
        if (!this._table.stateStorage || !this._table.stateKey) return;

        this._table.restoreState();
        this._fixRestoredFilters();
    }


    private _fixRestoredFilters(): void {
        Object.keys(this._table.filters).forEach(filterKey => {
            const filterMetadata = this._table.filters[filterKey] as FilterMetadata;
            const column = this._table.columns.find(x => x.field === filterKey);
            this._fixIfDateFilter(filterMetadata, filterKey, column);
            this._fixIfDropDownFilter(filterMetadata, filterKey, column);
        });
    }

    private _fixIfDateFilter(filterMetadata: FilterMetadata, field: string, column?: IGridColumn): void {
        let isDate = false;
        if (this._filterComponent) {
            const externalFilter = this._filterComponent.filters[
                Object.keys(this._filterComponent.filters)
                    .find(x => this._filterComponent.filters[x].field === field)];
            if (externalFilter) {
                isDate = externalFilter.settings.filterType == FilterType.Date;
            }
        } else {
            isDate = column?.filterSettings?.filterType == FilterType.Date;
        }

        if (isDate) {
            if (filterMetadata.matchMode == FilterMatchMode.BETWEEN && Array.isArray(filterMetadata.value)) {
                filterMetadata.value.forEach((val, index) =>
                    filterMetadata.value[index] = val ? new Date(val) : null);
            } else {
                filterMetadata.value = filterMetadata.value ? new Date(filterMetadata.value) : null;
            }
        }
    }

    private _fixIfDropDownFilter(filterMetadata: FilterMetadata, field: string, column?: IGridColumn): void {
        let options: SelectItem[] = null;
        if (this._filterComponent) {
            const externalFilter = this._filterComponent.filters[
                Object.keys(this._filterComponent.filters)
                    .find(x => this._filterComponent.filters[x].field === field)];
            if (externalFilter?.settings.inputType == FilterInputType.Dropdown) {
                options = externalFilter.settings.dropdownOptions;
            }
        } else {
            if (column?.filterSettings?.inputType == FilterInputType.Dropdown) {
                options = column.filterSettings?.dropdownOptions;
            }
        }

        if (options) {
            if (options.every(x => x.value !== filterMetadata.value)) {
                delete this._table.filters[field];
            }
        }
    }
}
