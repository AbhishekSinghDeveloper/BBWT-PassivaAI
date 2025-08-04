import { AfterViewInit, Component, ViewChild } from "@angular/core";

import { FilterInputType, FilterType } from "@features/filter";
import {
    CellEditInputType,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    IGridColumn,
    IGridSettings,
    ITableSettings
} from "@features/grid";
import { OrderService } from "@demo/northwind";
import { match } from "assert";


@Component({
    selector: "grid-local",
    templateUrl: "./grid-local.component.html",
    styleUrls: ["./grid-local.component.scss"]
})
export class GridLocalComponent implements AfterViewInit {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "id",
                header: "Id",
                cellEditingInputType: CellEditInputType.Number,
                filterSettings: {
                    filterType: FilterType.Numeric,
                    inputType: FilterInputType.Number
                },
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                field: "orderDate",
                header: "Order Date",
                displayMode: DisplayMode.Date,
                cellEditingInputType: CellEditInputType.Calendar,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            {
                field: "requiredDate",
                header: "Required Date",
                displayMode: DisplayMode.Date,
                cellEditingInputType: CellEditInputType.Calendar,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            {
                field: "shippedDate",
                header: "Shipped Date",
                displayMode: DisplayMode.Date,
                cellEditingInputType: CellEditInputType.Calendar,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            {
                field: "isPaid",
                header: "Is Paid",
                displayMode: DisplayMode.Conditional,
                displayConditionalTrueValue: "Yes",
                displayConditionalFalseValue: "No",
                sortable: false,
                cellEditingInputType: CellEditInputType.Checkbox,
                filterSettings: {
                    filterType: FilterType.Boolean,
                    inputType: FilterInputType.Checkbox,
                    ignoreIfConvertibleToFalse: true
                }
            },
            {
                field: "customerCode",
                header: "Customer Code",
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            }
        ],
        lazy: false
    };
    gridSettings: IGridSettings = {
        filtersRow: true,
    };

    @ViewChild("grid") private grid: GridComponent;

    constructor(private orderService: OrderService) {}

    ngAfterViewInit(): void {
        this.init();
    }

    private async init(): Promise<void> {
        this.grid.setTableProperty("value", await this.orderService.getAll());
    }
}