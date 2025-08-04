import { Component } from "@angular/core";
import { IGridColumn, IGridSettings, ITableSettings } from "@features/grid";

@Component({
    selector: "widget-grid-custom-format-tooltip",
    templateUrl: "./widget-grid-custom-format-tooltip.component.html"
})
export class WidgetGridCustomFormatTooltipComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "syntax", header: "Syntax" },
            { field: "description", header: "Description" }
        ],
        value: [
            {
                id: 1,
                syntax: "{col}",
                description: "Inserts the value of the current column into the cell."
            },
            {
                id: 4,
                syntax: "<u>{col}</u>",
                description: "Underlines the value of the current column."
            },
            {
                id: 2,
                syntax: "{Table1.Field1}",
                description: "Inserts a Field1 column value of Table1 into a cell."
            },
            {
                id: 2,
                syntax: "<a href='{Table1.Field1}' class='{Table1.Field2}'>{Table2.Field}</a>",
                description: "Creates a link with dynamic href and class based on field values of multiple columns."
            }
        ],
        lazy: false,
        paginator: false
    };
    gridSettings: IGridSettings = {
        readonly: true,
        sortEnabled: false
    };
}
