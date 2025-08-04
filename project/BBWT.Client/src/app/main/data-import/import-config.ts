import { ColumnDefinition } from "./column-definition";

export class ImportConfig {

    columnDefinitions: ColumnDefinition[];
    firstRow: number;
    lastRow?: number;
    sheetName?: string;
    maxErrorsCount: number;
    skipInvalidRows: boolean;
    data?: any;

    constructor(obj: {
        columnDefinitions: ColumnDefinition[],
        firstRow: number,
        lastRow?: number,
        sheetName?: string,
        maxErrorsCount?: number,
        skipInvalidRows?: boolean,
        data?: any
    }) {
        this.columnDefinitions = obj.columnDefinitions;
        this.firstRow = obj.firstRow;
        this.lastRow = obj.lastRow;
        this.sheetName = obj.sheetName;
        this.maxErrorsCount = obj.maxErrorsCount || 10;
        this.skipInvalidRows = obj.skipInvalidRows || false;
        this.data = obj.data;
    }
}