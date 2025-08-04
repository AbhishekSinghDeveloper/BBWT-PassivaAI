import { IGridColumn } from "@features/grid";

export type GridExportDataRow = { rowOutput: {[key: string]: any}, rowData: any };

export interface IGridExportData {
    columns: IGridColumn[];
    data: GridExportDataRow[];
}