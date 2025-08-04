import { Component, ViewChild } from "@angular/core";
import { Validators } from "@angular/forms";

import { MessageService, SelectItem } from "primeng/api";

import { Message } from "@bbwt/classes";
import { CustomerService, OrderService } from "../services";
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
    templateUrl: "./orders.component.html"
})
export class OrdersComponent {
    tableSettings: ITableSettings = {
        lazyLoadOnInit: false
    };
    gridSettings: IGridSettings = {
        deleteAllEnabled: true,
        updateMode: UpdateMode.Inline,
        filtersRow: true,
        exportEnabled: true
    };

    @ViewChild("grid", { static: true }) grid: GridComponent;


    constructor(public service: OrderService,
                private customersService: CustomerService,
                private messageService: MessageService) {
        this.init();
    }


    onGenerationCompleted(): void {
        this.grid.reload();
        this.messageService.add(Message.Success("Generation successful"));
    }


    private async init(): Promise<void> {
        this.gridSettings.dataService = this.service;

        const customerOptions = (await this.customersService.getAll()).map(x => <SelectItem>{ label: x.code, value: x.id });

        this.tableSettings = {
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
                    header: "Customer Code",
                    field: "customerId",
                    sortable: false,
                    cellEditingInputType: CellEditInputType.Dropdown,
                    dropdownOptions: customerOptions,
                    placeholder: "Select customer",
                    validators: [ new GridValidator(Validators.required) ],
                    filterSettings: {
                        filterType: FilterType.Numeric,
                        inputType: FilterInputType.Dropdown,
                        dropdownOptions: customerOptions
                    },
                    viewSettings: new GridColumnViewSettings({ width: "150px" })
                },
                {
                    header: "Employee",
                    field: "employee.name",
                    editable: false
                },
                {
                    header: "Order Date",
                    field: "orderDate",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "L LTS",
                    cellEditingInputType: CellEditInputType.Calendar,
                    filterSettings: {
                        filterType: FilterType.Date,
                        inputType: FilterInputType.Calendar
                    }
                },
                {
                    header: "Required Date",
                    field: "requiredDate",
                    displayMode: DisplayMode.Date,
                    cellEditingInputType: CellEditInputType.Calendar,
                    filterSettings: {
                        filterType: FilterType.Date,
                        inputType: FilterInputType.Calendar
                    }
                },
                {
                    header: "Shipped Date",
                    field: "shippedDate",
                    displayMode: DisplayMode.Date,
                    cellEditingInputType: CellEditInputType.Calendar,
                    filterSettings: {
                        filterType: FilterType.Date,
                        inputType: FilterInputType.Calendar
                    }
                },
                {
                    header: "Is Paid",
                    field: "isPaid",
                    sortable: false,
                    cellEditingInputType: CellEditInputType.Checkbox,
                    displayHandler: (isPaid: boolean) => (isPaid ? "Yes" : "No"),
                    filterSettings: {
                        filterType: FilterType.Boolean,
                        inputType: FilterInputType.Checkbox,
                        ignoreIfConvertibleToFalse: true
                    }
                }
            ]
        };
    }
}
