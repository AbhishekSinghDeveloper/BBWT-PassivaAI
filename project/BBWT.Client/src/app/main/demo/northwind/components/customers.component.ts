import { Component, ViewChild } from "@angular/core";

import { MessageService } from "primeng/api";

import { Message } from "@bbwt/classes";
import { CustomerService } from "../services";
import { notEmptyValidator } from "@bbwt/modules/validation";
import { GridColumnViewSettings, GridComponent, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";
import { FilterInputType, FilterType } from "@features/filter";
import { GridValidator } from "@features/grid";
import { Validators } from "@angular/forms";


@Component({
    templateUrl: "./customers.component.html"
})
export class CustomersComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                header: "ID",
                field: "id",
                editable: false,
                filterSettings: {
                    filterType: FilterType.Numeric,
                    inputType: FilterInputType.Number
                },
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                header: "Code",
                field: "code",
                validators: [
                    new GridValidator(Validators.required),
                    new GridValidator(notEmptyValidator())
                ],
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                header: "Company Name",
                field: "companyName",
                placeholder: "Company name"
            }
        ]
    };
    gridSettings: IGridSettings = {
        deleteAllEnabled: true,
        updateMode: UpdateMode.Inline,
        filtersRow: true,
        exportEnabled: true
    };

    @ViewChild("grid", { static: true }) grid: GridComponent;


    constructor(public service: CustomerService, private messageService: MessageService) {
        this.gridSettings.dataService = service;
    }


    onGenerationCompleted(): void {
        this.grid.reload();
        this.messageService.add(Message.Success("Generation successful"));
    }
}
