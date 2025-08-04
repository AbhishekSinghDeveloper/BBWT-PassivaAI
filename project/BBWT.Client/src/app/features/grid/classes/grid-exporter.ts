import { GridComponent } from "../grid.component";
import { ExportFormat } from "../enums/export-format";
import { IGridColumn } from "../interfaces/grid-column";
import { GridExportDataRow, IGridExportData } from "../interfaces/grid-export-data";

import { MenuItem } from "primeng/api";

import { jsPDF } from "jspdf";
import * as XLSX from "xlsx";
import autoTable from "jspdf-autotable";


export class GridExporter {
    menuItems: MenuItem[];

    private readonly _defaultFileName = "export";


    constructor(private _grid: GridComponent) {
        this.refresh();
    }


    private get fileName(): string {
        return !this._grid.gridSettings?.exportFileName
            ? this._defaultFileName
            : this._grid.gridSettings.exportFileName;
    }


    async export(format: ExportFormat): Promise<void> {
        const exportableColumns = (this._grid._table.columns as IGridColumn[])
            .filter(x => this._grid._isColumnVisible(x) && x.exportable !== false &&
                !this._grid._dataViewer.hasCustomTemplate(x.field));

        let data: any[];
        if (this._grid.gridSettings.exportSelectionOnly) {
            data = this._grid._table.selection
                ?  (this._grid._table.selectionMode == "multiple"
                    ? this._grid._table.selection
                    : [ this._grid._table.selection ])
                : [];
        } else {
            if (!this._grid._table?.lazy || !this._grid.gridSettings.dataService) {
                data = (this._grid._table.filteredValue || this._grid._table.value) ?? [];

                if (this._grid._table.frozenValue) {
                    data = [
                        ...this._grid._table.frozenValue,
                        ...data
                    ];
                }
            } else {
                const lazyLoadMetadata = this._grid._queryManager.getLazyLoadMetadata();
                lazyLoadMetadata.first = null;
                lazyLoadMetadata.rows = null;
                data = (await this._grid._crudManager.getData(lazyLoadMetadata)).items;
            }
        }

        const dataRows: GridExportDataRow[] = data.map(rowData => {
            const rowOutput = {};
            exportableColumns.forEach(columnItem => {
                const outputValue = this._grid._dataViewer.getCellDisplayValue(rowData, columnItem);
                rowOutput[columnItem.field] = outputValue ?? "";
            });
            return { rowData: rowData, rowOutput: rowOutput };
        });

        let exportData = { columns: exportableColumns, data: dataRows } as IGridExportData;
        if (this._grid.gridSettings.transformExportData) {
            exportData = await this._grid.gridSettings.transformExportData(exportData);
        }

        const body = exportData.data
            .map(exportDataRow => exportData.columns
                .map(columnItem => String(exportDataRow.rowOutput[columnItem.field])));
        const head = exportData.columns.map(columnItem => columnItem.header);

        switch (format) {
            case ExportFormat.CSV:
                this.exportCsv(head, body);
                break;
            case ExportFormat.PDF:
                this.exportPdf(head, body);
                break;
            case ExportFormat.XLSX:
                this.exportXlsx(head, body);
                break;
        }
    }

    refresh(): void {
        this.menuItems = [];

        if (!this._grid.gridSettings?.exportEnabled) return;

        if (!this._grid.gridSettings.exportAllowedFormats?.length ||
            this._grid.gridSettings.exportAllowedFormats.some(x => x === ExportFormat.CSV)){
            this.menuItems.push({
                label: "CSV",
                icon: "pi pi-file-o",
                command: () => this.export(ExportFormat.CSV)
            });
        }

        if (!this._grid.gridSettings.exportAllowedFormats?.length ||
            this._grid.gridSettings.exportAllowedFormats.some(x => x === ExportFormat.PDF)){
            this.menuItems.push({
                label: "PDF",
                icon: "pi pi-file-pdf",
                command: () => this.export(ExportFormat.PDF)
            });
        }

        if (!this._grid.gridSettings.exportAllowedFormats?.length ||
            this._grid.gridSettings.exportAllowedFormats.some(x => x === ExportFormat.XLSX)){
            this.menuItems.push({
                label: "XLSX",
                icon: "pi pi-file-excel",
                command: () => this.export(ExportFormat.XLSX)
            });
        }
    }

    /** Converts a value to how it should output in a CSV file
        Lint rules:
        - a cell is always wrapped with double quotes
        - all double quotes within a cell value are replaced with two double quotes ("")
          E.g. "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
        Note! The method doesn't sanitize data for Excel purposes. For Excel there should be additional measures taken.
        Useful references:
        - https://owasp.org/www-community/attacks/CSV_Injection
        - https://csvlint.io 
     */
    private sanitizeCsvData(data: Array<string[]>): void {
        for (let i = 0; i < data.length; i++) {
            for (let j = 0; j < data[i].length; j++) {
                data[i][j] = `"${data[i][j].replace("\"", "\"\"")}"`;
            }
        }
    }

    private exportCsv(headers: string[], data: Array<string[]>): void {
        this.sanitizeCsvData(data);

        const csvContent =
            headers.join(",") +
            "\n" +
            data.map(rowItem => rowItem.join(",")).join("\n");

        this.performDownload(this.fileName, ExportFormat.CSV, csvContent);
    }

    private exportPdf(headers: string[], data: Array<string[]>): void {
        const doc = new jsPDF("l", "mm", "a4");

        autoTable(doc, {
            head: [headers],
            body: data,
            styles: {
                cellWidth: "auto",
                minCellWidth: 30
            },
            horizontalPageBreak: true,
            horizontalPageBreakBehaviour: "immediately"
        });

        doc.save(this.fileName + ".pdf");
    }

    private exportXlsx(headers: string[], data: Array<string[]>): void {
        const wb = XLSX.utils.book_new();
        wb.SheetNames.push(this.fileName);
        wb.Sheets[this.fileName] = XLSX.utils.aoa_to_sheet([headers].concat(data));
        const xlsxData = XLSX.write(wb, { bookType: "xlsx", type: "array" });

        this.performDownload(this.fileName, ExportFormat.XLSX, xlsxData);
    }

    private performDownload(fileName: string, exportFormat: ExportFormat, data: any): void {
        const downloadLink = document.createElement("a");
        downloadLink.style.display = "none";

        switch (exportFormat) {
            case ExportFormat.CSV:
                downloadLink.href = "data:text/csv;charset=utf-8," + encodeURIComponent(data);
                downloadLink.setAttribute("download", fileName + ".csv");
                break;
            case ExportFormat.XLSX:
                downloadLink.href = window.URL.createObjectURL(
                    new Blob([data], {
                        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8"
                    })
                );
                downloadLink.setAttribute("download", fileName + ".xlsx");
                break;
        }

        document.body.appendChild(downloadLink);
        downloadLink.click();
        downloadLink.remove();
    }
}
