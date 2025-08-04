import { CellEditInputType, DisplayMode, IGridColumn } from "@features/grid";

import * as moment from "moment";

import { GridDataViewer } from "./grid-data-viewer";


export class GridHelper {
    static convertCellRawValueToString(cellRawValue: any, column: IGridColumn): string {
        switch (column.cellEditingInputType) {
            case CellEditInputType.Dropdown:
                const option = column.dropdownOptionsDataKey
                    ? column.dropdownOptions.find(x => x.value[column.dropdownOptionsDataKey] == cellRawValue[column.dropdownOptionsDataKey])
                    : column.dropdownOptions.find(x => x.value == cellRawValue);
                return option?.label || "";
            case CellEditInputType.Multiselect:
                if (!cellRawValue || !Array.isArray(cellRawValue)) return "";
                const values = column.dropdownOptionsDataKey
                    ? (cellRawValue as any[]).map(x => x[column.dropdownOptionsDataKey])
                    : (cellRawValue as any[]);
                const options = column.dropdownOptionsDataKey
                    ? column.dropdownOptions.filter(x => values.some(y => y == x.value[column.dropdownOptionsDataKey]))
                    : column.dropdownOptions.filter(x => values.some(y => y == x.value));
                return options.map(x => x.label).join(", ");
        }

        switch (column.displayMode) {
            case DisplayMode.Conditional:
                if (cellRawValue) {
                    return column.displayConditionalTrueValue || GridDataViewer.DisplayModeConditionDefaultTrueValue;
                }
                if (cellRawValue === false || column.countNullAsFalse) {
                    return column.displayConditionalFalseValue || GridDataViewer.DisplayModeConditionDefaultFalseValue;
                }
                return "";
            case DisplayMode.Date:
                if (!cellRawValue) return "";
                return moment(cellRawValue).format(!column.displayDateMomentFormat
                    ? GridDataViewer.DisplayModeDateDefaultMomentFormat
                    : column.displayDateMomentFormat);
            case DisplayMode.Number:
                return column.decimalPlaces && !isNaN(cellRawValue)
                    ? Number.parseFloat(cellRawValue).toFixed(column.decimalPlaces)
                    : (cellRawValue == null ? "" : String(cellRawValue));
            default: return cellRawValue == null ? "" : String(cellRawValue);
        }
    }

    static getCellDisplayValue(rowData: any, column: IGridColumn): any {
        const cellRawValue = GridHelper.getColumnFieldValue(rowData, column);

        if (!column) return cellRawValue;

        if (column.displayHandler) return column.displayHandler(cellRawValue, rowData);

        return GridHelper.convertCellRawValueToString(cellRawValue, column);
    }

    static getColumnFieldValue(rowData: any, column: IGridColumn): any {
        let result: any = rowData;
        column.field.split(".").forEach(x => result = result == null ? null : result[x]);
        return result;
    }
}