import { CellDataType } from "./cell-data-type";
import { CellDataTypeInfo } from "./cell-data-type-info";

export class ColumnDefinition {
    orderNumber: number;
    targetFieldName: string;
    type: CellDataType;
    typeInfo: CellDataTypeInfo;
    position: number;
    isAllowNulls: boolean;
    defaultValue?: string;

    constructor(orderNumber: number) {
        this.orderNumber = orderNumber;
        this.targetFieldName = `Field_${orderNumber}`;
        this.type = CellDataType.String;
        this.typeInfo = new CellDataTypeInfo();
        this.position = 1;
        this.isAllowNulls = true;
    }
}