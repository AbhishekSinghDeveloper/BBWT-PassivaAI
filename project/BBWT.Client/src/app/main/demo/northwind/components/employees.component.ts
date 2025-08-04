import { Component, ViewChild } from "@angular/core";
import { Validators } from "@angular/forms";

import { MessageService } from "primeng/api";

import { Message } from "@bbwt/classes";
import { EmployeeService } from "../services";
import { notEmptyValidator, ValidationPatterns } from "@bbwt/modules/validation";
import {
    CellEditInputType,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    GridValidator,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { FilterInputType, FilterType } from "@features/filter";


@Component({
    templateUrl: "./employees.component.html"
})
export class EmployeesComponent {
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
                header: "Name",
                field: "name",
                validators: [
                    new GridValidator(Validators.required),
                    new GridValidator(notEmptyValidator())
                ]
            },
            {
                header: "Phone",
                field: "phone"
            },
            {
                header: "Email",
                field: "email",
                validators: [
                    new GridValidator(Validators.required),
                    new GridValidator(Validators.pattern(ValidationPatterns.email))
                ]
            },
            {
                header: "Registration Date",
                field: "registrationDate",
                displayMode: DisplayMode.Date,
                cellEditingInputType: CellEditInputType.Calendar,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                },
                validators: [
                    new GridValidator(Validators.required)
                ]
            },
            {
                header: "Job Role",
                field: "jobRole"
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


    constructor(public service: EmployeeService, private messageService: MessageService) {
        this.gridSettings.dataService = service;
    }


    onGenerationCompleted(): void {
        this.grid.reload();
        this.messageService.add(Message.Success("Generation successful"));
    }
}
