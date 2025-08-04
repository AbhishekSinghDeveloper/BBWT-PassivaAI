import { Component, ViewChild } from "@angular/core";
import { CurrencyPipe } from "@angular/common";

import { OrderDetailsService } from "../services";
import { CreateMode, GridColumnViewSettings, GridComponent, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";
import { FilterInputType, FilterType } from "@features/filter";
import { IOrderDetails } from "..";


@Component({
    templateUrl: "./order-details.component.html"
})
export class OrderDetailsComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                header: "ID",
                field: "id",
                editable: false,
                filterSettings: {
                    inputType: FilterInputType.Number,
                    filterType: FilterType.Numeric
                },
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                header: "Order ID",
                field: "orderId",
                filterSettings: {
                    inputType: FilterInputType.Number,
                    filterType: FilterType.Numeric
                },
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                header: "Quantity",
                field: "quantity",
                filterSettings: {
                    inputType: FilterInputType.Number,
                    filterType: FilterType.Numeric
                },
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                header: "Price",
                field: "price",
                filterSettings: {
                    inputType: FilterInputType.Number,
                    filterType: FilterType.Numeric,
                    numericInputMaxFractionDigits: 2
                },
                displayHandler: (_, row: IOrderDetails) => this.currencyPipe.transform(row.price, "GBP"),
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                header: "Product",
                field: "product.title",
                editable: false
            },
        ]
    };

    gridSettings: IGridSettings = {
        deleteAllEnabled: true,
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Disabled,
        filtersRow: true,
        exportEnabled: true
    };

    @ViewChild("grid", { static: true }) grid: GridComponent;


    constructor(public service: OrderDetailsService, private currencyPipe: CurrencyPipe) {
        this.gridSettings.dataService = service;
    }
}