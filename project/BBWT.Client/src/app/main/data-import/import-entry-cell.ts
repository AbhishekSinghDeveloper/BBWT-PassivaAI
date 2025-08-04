import { CellDataType } from "./cell-data-type";

export interface ImportEntryCell {
    value: any;
    targetFieldName: string;
    type: CellDataType;
    orderNumber: number;
    errorMessage: string;
}