import { Component } from "@angular/core";

import { EmployeeService } from "@demo/northwind";
import { DemoDataImportService } from "./demo-data-import.service";
import { CellDataType, CellDataTypeInfo, ImportConfig } from "@main/data-import";
import { DisplayMode, ExportFormat, GridHelper, IGridColumn, IGridSettings, ITableSettings } from "@features/grid";


@Component({
    selector: "import-result",
    templateUrl: "./import-result.component.html"
})
export class ImportResultComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "name", header: "Name"},
            { field: "age", header: "Age"},
            { field: "phone", header: "Phone"},
            { field: "email", header: "Email"},
            {
                field: "registrationDate",
                header: "Registration Date",
                displayMode: DisplayMode.Date
            },
            { field: "jobRole", header: "Job Role"},
        ],
        selectionMode: "multiple"
    };
    gridSettings: IGridSettings = {
        readonly: true,
        deleteAllEnabled: true,
        importEnabled: true,
        importAllowedFormats: ".csv,.xls,.xlsx",
        exportEnabled: true,
        exportAllowedFormats: [ExportFormat.CSV],
        exportSelectionOnly: true,
        visibleColumnsSelector: true,
        selectColumn: true,
        selectAllVisible: false,
        transformExportData: exportData => {
            const newExportingColumn = <IGridColumn> {
                field: "adult",
                header: "Adult",
                displayHandler: (cellValue, rowValue) => rowValue.age >= 18 ? "Yes" : "No"
            };
            exportData.columns.push(newExportingColumn);
            exportData.data.forEach(exportDataRow =>
                exportDataRow.rowOutput[newExportingColumn.field] =
                    GridHelper.getCellDisplayValue(exportDataRow.rowData, newExportingColumn));

            return Promise.resolve(exportData);
        }
    };


    constructor(private employeeService: EmployeeService,
                private dataImportService: DemoDataImportService) {
        const config = new ImportConfig({
            columnDefinitions: [
                {
                    orderNumber: 1, targetFieldName: "Name", type: CellDataType.String,
                    isAllowNulls: false, typeInfo: new CellDataTypeInfo(), position: 1
                },
                {
                    orderNumber: 2, targetFieldName: "Age", type: CellDataType.Number,
                    isAllowNulls: false, typeInfo: new CellDataTypeInfo(), position: 2
                },
                {
                    orderNumber: 3, targetFieldName: "Phone", type: CellDataType.Phone,
                    isAllowNulls: true, typeInfo: new CellDataTypeInfo(), position: 3
                },
                {
                    orderNumber: 4, targetFieldName: "Email", type: CellDataType.Email,
                    isAllowNulls: true, typeInfo: new CellDataTypeInfo(), position: 4
                },
                {
                    orderNumber: 5, targetFieldName: "RegistrationDate",
                    type: CellDataType.Date, isAllowNulls: false,
                    typeInfo: new CellDataTypeInfo(), position: 5
                },
                {
                    orderNumber: 6, targetFieldName: "JobRole", type: CellDataType.Custom,
                    isAllowNulls: true,
                    typeInfo: new CellDataTypeInfo({customValidation: "JobRole"}), position: 6
                }
            ],
            firstRow: 2
        });
        dataImportService.setConfig(config);

        this.gridSettings.dataService = employeeService;
        this.gridSettings.importService = dataImportService;
    }
}