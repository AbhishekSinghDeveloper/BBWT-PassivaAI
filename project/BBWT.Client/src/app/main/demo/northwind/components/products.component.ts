import { Component, ViewChild } from "@angular/core";
import { Validators } from "@angular/forms";

import { MessageService } from "primeng/api";

import { Message } from "@bbwt/classes";
import { ProductService } from "../services";
import { notEmptyValidator } from "@bbwt/modules/validation";
import { GridColumnViewSettings, GridComponent, GridValidator, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";
import { FilterInputType, FilterType } from "@features/filter";


@Component({
    templateUrl: "./products.component.html"
})
export class ProductsComponent {
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
                header: "Title",
                field: "title",
                validators: [
                    new GridValidator(Validators.required),
                    new GridValidator(notEmptyValidator())
                ]
            },
        ]
    };
    gridSettings: IGridSettings = {
        deleteAllEnabled: true,
        updateMode: UpdateMode.Inline,
        filtersRow: true,
        exportEnabled: true
    };

    @ViewChild("grid", { static: true }) grid: GridComponent;


    constructor(public service: ProductService, private messageService: MessageService) {
        this.gridSettings.dataService = service;
    }


    onGenerationCompleted(): void {
        this.grid.reload();
        this.messageService.add(Message.Success("Generation successful"));
    }
}
