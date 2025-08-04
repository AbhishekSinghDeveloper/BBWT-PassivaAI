import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from "@angular/core";
import { NgForm } from "@angular/forms";

import { SelectItem } from "primeng/api";
import * as moment from "moment";

import { IColumnMetadata, IColumnStaticData, IColumnType, IFolder, ITableMetadata } from "@main/dbdoc";
import { IHash } from "@bbwt/interfaces";
import {
    ISection,
    getExpandBehaviourEnumAsOptions,
    IQueryFilterSet,
    IQueryFilter,
    IGridView,
    IGridViewColumn,
    IQueryTableColumn,
    IFilterControl,
    getFullColumnName,
    buildQueryFiltersTree,
    IQueryFilterBinding,
    IQueryTable,
    IView, IQueryRule, getSectionDataViewTypeEnumAsOptions, IQueryTableJoin
} from "../reporting-models";
import { SectionService } from "../services/section.service";
import { ReportEditorComponent } from "./report-editor.component";
import { QueryBuilderComponent } from "./query-builder/query-builder.component";
import { SectionViewComponent } from "./section-view.component";
import { GridViewSettingsComponent } from "./view-builder/grid-view-settings.component";
import { FilterControlSettingsComponent } from "./view-builder/filter-control-settings.component";
import {
    IQueryBuilderController,
    IQueryBuilderHandler,
    IViewBuilderController,
    IViewBuilderHandler
} from "../interfaces";
import { QueryBuilderHandler, ViewBuilderHandler } from "../classes";
import { IQueryableTableSource } from "../model/queryable-table-sources-models";


export interface IQueryFilterDataMap {
    [key: string]: {
        betweenInputComponentValue?: any[],
        columnMetadata: IColumnMetadata,
        filterControl: IFilterControl,
        isBetween?: boolean,
        possibleFilterControls?: SelectItem[],
        queryFilter: IQueryFilter
    }
}

@Component({
    selector: "section-editor",
    templateUrl: "./section-editor.component.html",
    styleUrls: ["./section-editor.component.scss"]
})
export class SectionEditorComponent implements OnInit, IQueryBuilderController, IViewBuilderController {
    @Input() controllerLoading: boolean;
    @Output() columnsDeleted = new EventEmitter<{ sectionId: string, columnIds: string[] }>();
    @Input() section: ISection;
    @ViewChild(QueryBuilderComponent, { static: false }) queryBuilderComponent: QueryBuilderComponent;
    @ViewChild(FilterControlSettingsComponent, { static: false }) filterControlSettingsComponent: FilterControlSettingsComponent;
    @ViewChild(SectionViewComponent, { static: false }) sectionViewComponent: SectionViewComponent;
    @ViewChild(GridViewSettingsComponent, { static: false }) gridViewSettingsComponent: GridViewSettingsComponent;
    @ViewChild("generalForm", { static: false }) generalForm: NgForm;

    columnsMetadata: IColumnMetadata[];
    columnTypes: IColumnType[];
    dataViewOptions = getSectionDataViewTypeEnumAsOptions();
    filterControlsOptions: SelectItem[];
    filterTreeLoading: boolean;
    possibleFilterDataTypesOfInputTypesMap: IHash<SelectItem[]>;
    possibleFilterInputTypesOfClrTypesMap: IHash<SelectItem[]>;
    possibleQueryRulesOfClrTypesMap: IHash<SelectItem[]>;
    qbh: IQueryBuilderHandler;
    queryColumnsMetadataMap: IHash<IColumnMetadata>;
    queryColumnsOptions: SelectItem[];
    queryFiltersRelatedDataMap: IQueryFilterDataMap;
    queryRules: IQueryRule[];
    queryTablesMetadataMap: IHash<ITableMetadata>;
    queryTreeLoading: boolean;
    tablesMetadata: ITableMetadata[];
    vbh: IViewBuilderHandler;

    _builderTabFirstTimeOpened = false;
    _expandBehaviourOptions = getExpandBehaviourEnumAsOptions();
    _generalSavingRequired = false;
    _previewRefreshed: boolean;
    _previewTabFirstOpened = false;
    _sectionGeneralBeforeSaveTimeout: any;
    _sectionGeneralSaveTimeout: any;
    _sectionGeneralSaving: boolean;


    constructor(public reportEditorComponent: ReportEditorComponent, private sectionService: SectionService) {
    }


    get folders(): IFolder[] {
        return this.reportEditorComponent.folders;
    };


    async addDuplicateQueryTable(queryTableJoin: IQueryTableJoin): Promise<void> {
        await this.qbh.addDuplicateQueryTable(queryTableJoin);
        await this.refreshQueryStructureRelatedData();
        this.queryBuilderComponent.requestRawSql();
        this.gridViewSettingsComponent.refreshAllColumnsSwitchers();

        this._previewRefreshed = false;
    }

    addFilterControl(filterControl: IFilterControl): Promise<void> {
        return this.vbh.addFilterControl(filterControl).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this.filterControlSettingsComponent.refresh();
            this._previewRefreshed = false;
        });
    }

    addQueryFilter(parentQueryFilterSet: IQueryFilterSet, queryFilter: IQueryFilter): Promise<void> {
        return this.qbh.addQueryFilter(parentQueryFilterSet, queryFilter).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this._previewRefreshed = false;
        });
    }

    addQueryFilterSet(parentQueryFilterSet: IQueryFilterSet): Promise<void> {
        return this.qbh.addQueryFilterSet(parentQueryFilterSet)
            .then(() => this.refreshQueryFiltersRelatedData());
    }

    addQueryTable(tableMetadataId: number): Promise<IQueryTable> {
        return this.qbh.addQueryTable(tableMetadataId)
            .then(result => {
                this._onAddQueryTableOrColumnAddedHandler();
                return result;
            });
    }

    addQueryTablesFromSource(sources: IQueryableTableSource[]): Promise<void> {
        return this.qbh.addQueryTablesFromSource(sources)
            .then(() => this._onAddQueryTableOrColumnAddedHandler());
    }

    addQueryTableColumn(columnMetadataId: number, parentQueryTableId?: number): Promise<void> {
        return this.qbh.addQueryTableColumn(columnMetadataId, parentQueryTableId)
            .then(() => this._onAddQueryTableOrColumnAddedHandler());
    }

    addQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<IQueryTableJoin> {
        return this.qbh.addQueryTableJoin(queryTableJoin)
            .then(queryTableJoin => {
                this.refreshQueryStructureRelatedData();
                this.queryBuilderComponent.requestRawSql();
                this._previewRefreshed = false;

                return queryTableJoin;
            });
    }

    bindFilterControlToQueryFilter(filterControlId: string, queryFilter: IQueryFilter): Promise<void> {
        return this.qbh.bindFilterControlToQueryFilter(filterControlId, queryFilter)
            .then(() => {
                this.refreshQueryFiltersRelatedData();
                this.queryBuilderComponent.requestRawSql();
                this.filterControlSettingsComponent.refresh();
                this._previewRefreshed = false;
            });
    }

    performGeneralSaving(): Promise<void> {
        if (this._sectionGeneralBeforeSaveTimeout) {
            clearTimeout(this._sectionGeneralBeforeSaveTimeout);
            this._sectionGeneralBeforeSaveTimeout = null;
        }

        if (this._sectionGeneralSaveTimeout) {
            clearTimeout(this._sectionGeneralSaveTimeout);
            this._sectionGeneralSaveTimeout = null;
        }

        return this._generalSavingRequired
            ? this.reportEditorComponent.updateSection(this.section)
                .then(() => {
                    this._previewRefreshed = false;
                })
                .finally(() => {
                    this._generalSavingRequired = false;
                    this._sectionGeneralSaving = false;
                })
            : Promise.resolve();
    }

    checkMasterDetailsFilters(deletedColumnIds: string[]): void {
        this.section.query.rootFilterSet.queryFilters = this.section.query.rootFilterSet.queryFilters
            .filter(x => x.queryFilterBindings.every(y => !y || y.bindingType !== "masterDetailGrid" ||
                !deletedColumnIds.includes(y.masterDetailQueryTableColumnId)));

        this.refreshQueryFiltersRelatedData();
    }

    deleteFilterControl(filterControl: IFilterControl, deleteLinkedQueryFilters: boolean): Promise<void> {
        return this.vbh.deleteFilterControl(filterControl, deleteLinkedQueryFilters).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this.filterControlSettingsComponent.refresh();
            this._previewRefreshed = false;
        });
    }

    deleteQueryFilter(queryFilter: IQueryFilter): Promise<void> {
        return this.qbh.deleteQueryFilter(queryFilter).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this.filterControlSettingsComponent.refresh();
            this._previewRefreshed = false;
        });
    }

    deleteQueryFilterBinding(queryFilterBinding: IQueryFilterBinding): Promise<void> {
        return this.qbh.deleteQueryFilterBinding(queryFilterBinding).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this.filterControlSettingsComponent.refresh();
            this._previewRefreshed = false;
        });
    }

    deleteQueryFilterSet(queryFilterSet: IQueryFilterSet): Promise<void> {
        return this.qbh.deleteQueryFilterSet(queryFilterSet).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this.filterControlSettingsComponent.refresh();
            this._previewRefreshed = false;
        });
    }

    deleteQueryTable(queryTable: IQueryTable): Promise<void> {
        const deletedColumnIds = queryTable.columns.map(x => x.id);

        return this.qbh.deleteQueryTable(queryTable).then(() =>
            this._onDeleteQueryTableOrColumnHandler(deletedColumnIds));
    }

    deleteQueryTableColumn(queryTableColumn: IQueryTableColumn): Promise<void> {
        return this.qbh.deleteQueryTableColumn(queryTableColumn).then(() =>
            this._onDeleteQueryTableOrColumnHandler([queryTableColumn.id]));
    }

    deleteQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<void> {
        return this.qbh.deleteQueryTableJoin(queryTableJoin).then(() => {
            this.refreshQueryStructureRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this._previewRefreshed = false;
        });
    };

    loadFolderStructure(folderId?: string): Promise<void> {
        return this.reportEditorComponent.loadFolderStructure(folderId);
    }

    loadFullTableMetadata(tableMetadataId: number): Promise<ITableMetadata> {
        return this.reportEditorComponent.fetchFullTableMetadata(tableMetadataId);
    }

    moveFilterControl(fromIndex: number, toIndex: number): Promise<void> {
        return this.vbh.moveFilterControl(fromIndex, toIndex).then(() => {
            this._previewRefreshed = false;
        });
    }

    moveGridViewColumn(fromIndex: number, toIndex: number): Promise<void> {
        return this.vbh.moveGridViewColumn(fromIndex, toIndex).then(() => {
            this._previewRefreshed = false;
        });
    }

    toggleAllGridViewColumnsSortable(value: boolean): Promise<void> {
        return this.vbh.toggleAllGridViewColumnsSortable(value).then(() => {
            this.section.view.gridView.viewColumns.forEach(x => x.sortable = value);
            this._previewRefreshed = false;
        });
    }

    toggleAllGridViewColumnsVisible(value: boolean): Promise<void> {
        return this.vbh.toggleAllGridViewColumnsVisible(value).then(() => {
            this.section.view.gridView.viewColumns.forEach(x => {
                const columnMetadata = this.queryColumnsMetadataMap[x.queryTableColumnId]?.staticData;
                if (!columnMetadata?.isPrimaryKey && !columnMetadata?.isForeignKey) {
                    x.visible = value;
                }
            });
            this._previewRefreshed = false;
        });
    }

    ngOnInit(): void {
        this.columnsMetadata = this.reportEditorComponent.columnsMetadata;
        this.columnTypes = this.reportEditorComponent.columnTypes;
        this.possibleFilterDataTypesOfInputTypesMap = this.reportEditorComponent.possibleFilterDataTypesOfInputTypesMap;
        this.possibleFilterInputTypesOfClrTypesMap = this.reportEditorComponent.possibleFilterInputTypesOfClrTypesMap;
        this.possibleQueryRulesOfClrTypesMap = this.reportEditorComponent.possibleQueryRulesOfClrTypesMap;
        this.queryRules = this.reportEditorComponent.queryRules;
        this.tablesMetadata = this.reportEditorComponent.tablesMetadata;

        this.qbh = new QueryBuilderHandler(this.sectionService, this.reportEditorComponent.report, this.section);
        this.vbh = new ViewBuilderHandler(this.sectionService, this.reportEditorComponent.report, this.section);
    }

    refreshQueryFiltersRelatedData(refreshChildren = true): void {
        this.queryFiltersRelatedDataMap = {};

        if (!this.reportEditorComponent.folders || !this.section.query?.dbDocFolderId || !this.section.view?.filters) return;

        const queryFilters = this.section.query.queryFilterSets
            .reduce((accumulator, current) => accumulator.concat(current.queryFilters), []);

        this.filterControlsOptions = this.section.view.filters.map(y => <SelectItem>{ label: y.name, value: y.id });

        const currentFolder = this.reportEditorComponent.folders
            .find(x => x.id == this.section.query.dbDocFolderId);
        if (currentFolder) {
            queryFilters.forEach(x => this.refreshQueryFilterRelatedData(x));
        }

        if (refreshChildren) {
            this.queryBuilderComponent.refreshQueryFiltersRelatedData();
        }
    }

    refreshQueryFilterRelatedData(queryFilter: IQueryFilter): void {
        if (queryFilter.queryRuleId) {
            this._setFilterQueryRule(queryFilter);
        }

        this.queryFiltersRelatedDataMap[queryFilter.id] = {
            queryFilter: queryFilter,
            columnMetadata: queryFilter.queryTableColumnId ?
                this.queryColumnsMetadataMap[queryFilter.queryTableColumnId]
                : null,
            isBetween: queryFilter.queryRule?.code === "between",
            possibleFilterControls: queryFilter.queryTableColumnId ?
                this.section.view.filters
                    .filter(y => this.reportEditorComponent.possibleFilterInputTypesOfClrTypesMap[
                        this.queryColumnsMetadataMap[queryFilter.queryTableColumnId].staticData.clrTypeGroup]
                        .some(z => z.value === y.inputType))
                    .map(y => <SelectItem>{ label: `@${y.name}`, value: y.id })
                : [],
            filterControl: this.section.view.filters.find(y =>
                y.queryFilterBindings.some(z => z.queryFilterId === queryFilter.id))
        };

        if (this.queryFiltersRelatedDataMap[queryFilter.id].isBetween) {
            if (this.queryFiltersRelatedDataMap[queryFilter.id].columnMetadata.staticData.clrTypeGroup === "numeric") {
                this.queryFiltersRelatedDataMap[queryFilter.id].betweenInputComponentValue = [
                    isNaN(queryFilter.value) ? null : Number(queryFilter.value),
                    isNaN(queryFilter.value2) ? null : Number(queryFilter.value2)
                ];
            } else if (this.queryFiltersRelatedDataMap[queryFilter.id].columnMetadata.staticData.clrTypeGroup === "date") {
                this.queryFiltersRelatedDataMap[queryFilter.id].betweenInputComponentValue = [];

                if (moment.isDate(queryFilter.value)) {
                    this.queryFiltersRelatedDataMap[queryFilter.id].betweenInputComponentValue.push(new Date(queryFilter.value));
                }

                if (moment.isDate(queryFilter.value2)) {
                    this.queryFiltersRelatedDataMap[queryFilter.id].betweenInputComponentValue.push(new Date(queryFilter.value2));
                }
            }
        }
    }

    async refreshQueryStructureRelatedData(refreshChildren = true): Promise<void> {
        if (!this.reportEditorComponent.folders || !this.section.query?.dbDocFolderId) return;

        const queryColumns = this.section.query.queryTables
            .reduce((accumulator, current) => accumulator.concat(current.columns), []);
        // Named Query should add more options

        this.queryTablesMetadataMap = {};
        this.queryColumnsMetadataMap = {};
        const currentFolder = this.reportEditorComponent.folders
            .find(x => x.id == this.section.query.dbDocFolderId);
        if (currentFolder) {
            await this.reportEditorComponent.loadFolderStructure(currentFolder.id);

            this.section.query.queryTables.forEach((x: IQueryTable) =>
                this.queryTablesMetadataMap[x.id] = currentFolder.tables
                    .find((y: ITableMetadata) => x.sourceTableId === y.tableId));

            const currentFolderColumnsMetadata = currentFolder.tables
                .reduce((accumulator, current) => accumulator.concat(current.columns), []);

            //...
            queryColumns.forEach((x: IQueryTableColumn) => {
                this.queryColumnsMetadataMap[x.id] = currentFolderColumnsMetadata
                    .find((y: IColumnMetadata) => y.columnId == x.sourceColumnId);

                // TODO: temp. hack for forms
                if (this.queryColumnsMetadataMap[x.id] == null) {
                    this.queryColumnsMetadataMap[x.id] = <IColumnMetadata>{
                        staticData: <IColumnStaticData>{
                            clrTypeGroup: "string",
                            parentTableName: this.section.query.queryTables.find(y => y.id == x.queryTableId).sourceTableId,
                            columnName: x.sourceColumnId,
                            isForeignKey: false,
                            isPrimaryKey: false
                        }
                    }
                }
            });
        }

        this.queryColumnsOptions = queryColumns
            .filter((x: IQueryTableColumn) => !x.onlyForJoin)
            .map((x: IQueryTableColumn) =>
                <SelectItem> {
                    label: getFullColumnName(this.queryColumnsMetadataMap[x.id].staticData),
                    value: x.id
                });

        if (refreshChildren) {
            this.queryBuilderComponent.refreshQueryStructureRelatedData();
        }
    }

    requestRawSql(): Promise<string> {
        return this.sectionService.getSqlQuery(this.section.id)
            .then(result => this.formatSqlQueryView(result?.sql || ""));
    }    

    updateFilterControl(filterControl: IFilterControl): Promise<void> {
        return this.vbh.updateFilterControl(filterControl).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this.filterControlSettingsComponent.refresh();
            this._previewRefreshed = false;
        });
    }

    updateGridView(gridView: IGridView): Promise<void> {
        return this.vbh.updateGridView(gridView).then(() => {
            this._previewRefreshed = false;
        });
    }

    updateGridViewColumn(gridViewColumn: IGridViewColumn): Promise<void> {
        return this.vbh.updateGridViewColumn(gridViewColumn).then(() => {
            this._previewRefreshed = false;
        });
    }

    updateQueryFilter(queryFilter: IQueryFilter): Promise<void> {
        return this.qbh.updateQueryFilter(queryFilter).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this.filterControlSettingsComponent.refresh();
            this._previewRefreshed = false;
        });
    }

    updateMasterDetailFilterBinding(queryFilterBinding: IQueryFilterBinding): Promise<void> {
        return this.qbh.updateMasterDetailFilterBinding(queryFilterBinding).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this._previewRefreshed = false;
        });
    }

    updateQueryFilterSet(queryFilterSet: IQueryFilterSet): Promise<void> {
        return this.qbh.updateQueryFilterSet(queryFilterSet).then(() => {
            this.refreshQueryFiltersRelatedData();
            this.queryBuilderComponent.requestRawSql();
            this._previewRefreshed = false;
        });
    }

    updateQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<IQueryTableJoin> {
        return this.qbh.updateQueryTableJoin(queryTableJoin)
            .then(queryTableJoin => {
                this.refreshQueryStructureRelatedData();
                this.queryBuilderComponent.requestRawSql();
                this._previewRefreshed = false;

                return queryTableJoin;
            });
    }

    _onSectionTextGeneralDataChange(): void {
        if (this._sectionGeneralBeforeSaveTimeout) {
            clearTimeout(this._sectionGeneralBeforeSaveTimeout);
            this._sectionGeneralBeforeSaveTimeout = null;
        }

        this._generalSavingRequired = true;

        if (this.generalForm.valid) {
            this._sectionGeneralBeforeSaveTimeout = setTimeout(
                () => {
                    this._saveSectionGeneral();
                    this._sectionGeneralBeforeSaveTimeout = null;
                },
                2000);
        }
    }

    async _onSectionActiveTabChanged(index: number): Promise<void> {
        switch (index) {
            case 1:
                if (this.section.dataViewType === "noView") {
                    this._onPreviewTabActivated();
                } else {
                    await this._onBuilderTabActivated();
                }
                break;
            case 2:
                await this._onBuilderTabActivated();
                break;
            case 3:
                this._onPreviewTabActivated();
                break;
        }
    }

    _saveSectionGeneral(): void {
        if (this._sectionGeneralSaveTimeout) {
            clearTimeout(this._sectionGeneralSaveTimeout);
            this._sectionGeneralSaveTimeout = null;
        }

        this._sectionGeneralSaveTimeout = setTimeout(() => this._sectionGeneralSaveTimeout = null, 2000);

        this._sectionGeneralSaving = true;
        this.reportEditorComponent.updateSection(this.section)
            .then(() => this._previewRefreshed = false)
            .finally(() => {
                this._generalSavingRequired = false;
                this._sectionGeneralSaving = false;
            });
    }


    private _cssFormat(value: string) {
        return `<b>${value}</b>`;
    }

    private async _onAddQueryTableOrColumnAddedHandler(): Promise<void> {
        if (this.section.query.queryTables.length === 1) {
            this.section.query.dbDocFolderId = this.queryBuilderComponent?.selectedFolder?.id;
        }

        await this.refreshQueryStructureRelatedData();
        this.refreshQueryFiltersRelatedData();
        this.queryBuilderComponent.requestRawSql();
        this.gridViewSettingsComponent.refreshAllColumnsSwitchers();

        this._previewRefreshed = false;
    }

    private async _onBuilderTabActivated(): Promise<void> {
        if (this._builderTabFirstTimeOpened) return;

        this.queryTreeLoading = true;
        this.filterTreeLoading = true;

        await this.reportEditorComponent.loadFullSection(this.section);
        await this.reportEditorComponent.loadSectionMetadata(this.section);
        this._sortFilterControls(this.section.view);
        this._sortGridViewColumns(this.section.view.gridView);

        buildQueryFiltersTree(this.section.query);
        await this.refreshQueryStructureRelatedData(false);
        this.refreshQueryFiltersRelatedData(false);

        this.queryTreeLoading = false;
        this.filterTreeLoading = false;
        this._builderTabFirstTimeOpened = true;
    }

    private _onPreviewTabActivated(): void {
        if (this._generalSavingRequired) {
            this._saveSectionGeneral();
            clearTimeout(this._sectionGeneralBeforeSaveTimeout);
            this._sectionGeneralBeforeSaveTimeout = null;
        }

        if (!this._previewRefreshed) {
            if (this._previewTabFirstOpened && this.sectionViewComponent) {
                this.sectionViewComponent.refresh();
            }

            this._previewTabFirstOpened = true;
            this._previewRefreshed = true;
        }
    }

    private async _onDeleteQueryTableOrColumnHandler(columnIds: string[]): Promise<void> {
        this.columnsDeleted.emit({ sectionId: this.section.id, columnIds: columnIds });

        await this.refreshQueryStructureRelatedData();
        this.refreshQueryFiltersRelatedData();
        this.queryBuilderComponent.requestRawSql();
        this.filterControlSettingsComponent.refresh();
        this.gridViewSettingsComponent.refreshAllColumnsSwitchers();

        this._previewRefreshed = false;
    }

    private _setFilterQueryRule(queryFilter: IQueryFilter): void {
        queryFilter.queryRule = this.reportEditorComponent.queryRules
            .find(y => y.id === queryFilter.queryRuleId);
    }

    private _sortFilterControls(view: IView): void {
        view.filters.sort((a, b) => a.sortOrder - b.sortOrder);
    }

    private _sortGridViewColumns(gridView: IGridView): void {
        gridView.viewColumns.sort((a, b) => a.sortOrder - b.sortOrder);
    }

    // TODO: later should be more robust solution. Current one is VERY hacky - for demo purposes
    private formatSqlQueryView(sql: string): string {
        function cssFormat(v) {
            return `<b>${v}</b>`;
        }
        while (sql.includes("\n")) {
            sql = sql.replace("\n", " ");
        }
        sql = sql.replace("SELECT ", cssFormat("SELECT") + " ");
        sql = sql.replace(" FROM ", "<br/>" + cssFormat("FROM") + " ");
        sql = sql.replace(" WHERE ", "<br/>" + cssFormat("WHERE") + " ");

        while (sql.includes(" LEFT JOIN ")) {
            sql = sql.replace(" LEFT JOIN ", "<br/>" + cssFormat("LEFT JOIN") + " ");
        }
        while (sql.includes(" RIGHT JOIN ")) {
            sql = sql.replace(" RIGHT JOIN ", "<br/>" + cssFormat("RIGHT JOIN") + " ");
        }
        while (sql.includes(" INNER JOIN ")) {
            sql = sql.replace(" INNER JOIN ", "<br/>" + cssFormat("INNER JOIN") + " ");
        }
        while (sql.includes(" OUTER JOIN ")) {
            sql = sql.replace(" OUTER JOIN ", "<br/>" + cssFormat("OUTER JOIN") + " ");
        }

        return sql;
    }
}