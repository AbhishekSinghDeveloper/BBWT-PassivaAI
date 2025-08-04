import { Component, Input, OnInit, ViewChild } from "@angular/core";

import {
    CellEditInputType,
    CreateMode,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    IGridColumn,
    IGridSettings,
    ITableSettings
} from "@features/grid";
import { FilterInputType, FilterType, IQueryCommand } from "@features/filter";
import { DbDocService } from "../dbdoc.service";
import {
    AnonymizationAction,
    IColumnMetadata,
    IDeleteTableEntityRequest,
    ISaveTableEntityRequest,
    ITableData,
    ITableMetadata
} from "../dbdoc-models";
import { getGridExternalMetadataFromTableMetadataResult } from "../metadata-converters";
import { DbExplorerComponent } from "@main/dbdoc/components/db-explorer.component";
import { DbDocTableDataService } from "../dbdoc-table-data.service";


@Component({
    selector: "table-metadata-editor",
    templateUrl: "./table-metadata-editor.component.html",
    styleUrls: ["./table-metadata-editor.component.scss"]
})
export class TableMetadataEditorComponent implements OnInit {
    _tableMetadata: ITableMetadata;
    @Input() set tableMetadata(value: ITableMetadata) {
        this._tableMetadata = value;

        this.updateColumnsGridByTableMetadata(value);
        this.updateTableDataGridByTableMetadata(value);
    }

    @Input() showTableData: boolean;
    @Input() readOnlyTableData: boolean;
    
    activeTabViewIndex = 0;
    tableAnonymizationOptions = [
        { value: AnonymizationAction.LeaveUnchanged, "label": "Leave data unchanged" },
        { value: AnonymizationAction.Anonymize, "label": "Anonymize" },
        { value: AnonymizationAction.Clear, "label": "Clear table" }]
    ;
    columnsTableSettings: ITableSettings;
    columnsGridSettings: IGridSettings = {
        readonly: true,
        sortEnabled: false
    };
    tableDataTableSettings: ITableSettings;
    tableDataGridSettings: IGridSettings;

    @ViewChild("gridTableData", { static: false }) private gridTableData: GridComponent;


    constructor(
        public dbExplorerComponent: DbExplorerComponent,
        private dbDocService: DbDocService,
        private dbDocTableDataService: DbDocTableDataService
    ) {
    }


    ngOnInit(): void {                
        this.updateColumnsGridByTableMetadata(this._tableMetadata);
        this.updateTableDataGridByTableMetadata(this._tableMetadata);
    }

    save(): void {
        const savingTableMetadata = {...this._tableMetadata};
        savingTableMetadata.columns = null;
        this.dbDocService.updateTableMetadata(savingTableMetadata.id, savingTableMetadata);
    }

    requestTableDataPage(queryCommand: IQueryCommand): Promise<ITableData> {
        return this.dbDocTableDataService.getTableData(this._tableMetadata.tableId, this._tableMetadata.folderId,
            queryCommand);
    }

    saveTableEntity(dataKey: any, entity: any): Promise<any> {
        return this.dbDocTableDataService.saveTableEntity(<ISaveTableEntityRequest>{
            entity, tableMetadataId: this._tableMetadata.id
        });
    }

    deleteTableEntity(dataKey: any): Promise<void> {
        return this.dbDocTableDataService.deleteTableEntity(<IDeleteTableEntityRequest>{
            uniqueTableId: this._tableMetadata.tableId, entityId: String(dataKey)
        });
    }

    onColumnMetadataChanged(columnMetadata: IColumnMetadata, toastNotify: boolean = true) {
        this.dbDocService.updateColumnMetadata(columnMetadata.id, columnMetadata, toastNotify);
    }

    private updateColumnsGridByTableMetadata(tableMetadata: ITableMetadata) {
        this.columnsTableSettings = this.getColumnsTableSettings(this._tableMetadata);
    }

    private getColumnsTableSettings(tableMetadata: ITableMetadata): ITableSettings {
        return <ITableSettings>{
            columns: <IGridColumn[]>[
                {
                    field: "staticData.columnName",
                    header: "Column Name",
                    viewSettings: new GridColumnViewSettings({ width: "250px" })
                },
                {
                    field: "staticData.type",
                    header: "Data Type",
                    viewSettings: new GridColumnViewSettings({ width: "150px" })
                },
                {
                    field: "staticData.allowNull",
                    header: "Allow Null",
                    displayMode: DisplayMode.Conditional,
                    displayConditionalTrueValue: "Yes",
                    displayConditionalFalseValue: "No",
                    viewSettings: new GridColumnViewSettings({ width: "90px" })
                },
                { field: "description", header: "Description" },
                {
                    field: "hidden",
                    header: "Hidden",
                    viewSettings: new GridColumnViewSettings({ width: "90px" })
                },
            ],
            value: tableMetadata.columns,
            paginator: false
        };
    }

    private updateTableDataGridByTableMetadata(tableMetadata: ITableMetadata) {
        this.tableDataTableSettings = this.getTableDataTableSettings(tableMetadata);
        this.tableDataGridSettings = this.getTableDataGridSettings(tableMetadata);
        this.gridTableData?.reset();
    }

    private getTableDataTableSettings(tableMetadata: ITableMetadata): ITableSettings {
        return <ITableSettings>{
            autoLayout: true,
            resizableColumns: true,
            //dataKey: "Id",
            //selectionMode: "single",
            columnResizeMode: "expand",
            styleClass: "p-datatable-gridlines p-datatable-striped",
            rows: 50,
            rowsPerPageOptions: [5, 10, 20, 50, 100]
        };
    }

    private getTableDataGridSettings(tableMetadata: ITableMetadata): IGridSettings {
        return <IGridSettings>{
            //selectColumn: true,
            createMode: CreateMode.Disabled,
            dataService: this,
            dataServiceGetPageMethodName: "requestTableDataPage",
            //dataServiceUpdateMethodName: "saveTableEntity",
            //dataServiceDeleteMethodName: "deleteTableEntity",
            readonly: true,
            //readonly: this.readOnlyTableData,
            loadedDataHandler: (data: ITableData) => {
                this.gridTableData.setTableProperty("columns", data.columns.map(x => this.getGridColumn(x)));
                return data.data;
            },
            externalMetadata: () => {
                return this.dbDocService.getTableMetadata(tableMetadata.tableId, tableMetadata.folderId)
                    .then(result => getGridExternalMetadataFromTableMetadataResult(result));
            }
        };
    }

    private getGridColumn(columnItem: { item1: string, item2: string }): IGridColumn {
        const gridColumn = <IGridColumn> {
            header: columnItem.item2,
            field: columnItem.item1
        };

        const columnMetadata = this._tableMetadata.columns.find(x => x.staticData.columnName == columnItem.item1);
        if (!!columnMetadata) {
            gridColumn.cellEditingInputType = this.getCellEditInputType(columnMetadata);
            gridColumn.displayMode = this.getDisplayMode(columnMetadata);
            gridColumn.editable = !columnMetadata.staticData.isPrimaryKey;
        }

        return gridColumn;
    }

    // TODO: need to recover getting correct type of column
    private getCellEditInputType(columnMetadata: IColumnMetadata): CellEditInputType {
        if (columnMetadata.staticData.isForeignKey) {
            return CellEditInputType.Text;
        }

        switch (columnMetadata.staticData.clrTypeGroup) {
            case "numeric": return CellEditInputType.Number;
            case "date": return CellEditInputType.Calendar;
            case "bool": return CellEditInputType.Checkbox;
            default: return CellEditInputType.Text;
        }
    }

    // TODO: need to recover getting correct type of column
    private getDisplayMode(columnMetadata: IColumnMetadata): DisplayMode {        
        switch (columnMetadata.staticData.clrTypeGroup) {
            case "date": return DisplayMode.Date;
            default: return DisplayMode.Text;
        }
    }
}