import { QueryList, TemplateRef } from "@angular/core";

import { GridComponent } from "../grid.component";
import { GridColumnTemplate } from "../grid-template.directive";
import { DisplayMode } from "../enums/display-mode";
import { IGridColumn } from "../interfaces/grid-column";
import { GridHelper } from "./grid-helper";


export class GridDataViewer {
    static readonly DisplayModeConditionDefaultTrueValue = "Yes";
    static readonly DisplayModeConditionDefaultFalseValue = "No";
    static readonly DisplayModeDateDefaultMomentFormat = "L";


    constructor(private _grid: GridComponent, private _columnTemplates: QueryList<GridColumnTemplate>) {}


    get displayModeEnum(): any {
        return DisplayMode;
    }


    getCellDisplayValue(rowData: any, column: IGridColumn): any {
        return GridHelper.getCellDisplayValue(rowData, column);
    }

    getColumnDisplayTemplate(columnName: string): TemplateRef<any> {
        const columnTemplate = this._columnTemplates
            .filter(x => x.gridColumnTemplate == "displayingRow")
            .find(x => x.columnName == columnName);

        if (!columnTemplate) {
            throw Error(`Custom column template is not defined for a column '${columnName}' - one is required for DisplayingMode.CustomTemplate for the column`);
        }

        return columnTemplate ? columnTemplate.template : null;
    }

    hasCustomTemplate(columnName: string): boolean {
        return this._columnTemplates
            .filter(x => x.gridColumnTemplate == "displayingRow")
            .some(x => x.columnName == columnName);
    }
}