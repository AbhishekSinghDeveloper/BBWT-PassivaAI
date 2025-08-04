import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { Validators } from "@angular/forms";

import { SelectItem } from "primeng/api";
import { FilterInputType, FilterType } from "@features/filter";
import {
    CellEditInputType,
    DisplayMode,
    GridValidator,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { IdHashingDemoService, SimpleOrdersService } from "./services";
import { Order } from "./models";
import { EntityId } from "@bbwt/interfaces";
import { CustomerService } from "../northwind";

@Component({
    templateUrl: "./orders.component.html",
    styleUrls: ["./orders.component.scss"]
})
export class OrdersComponent {
    tableSettings: ITableSettings;
    gridSettings: IGridSettings = {
        exportEnabled: true,
        additionalRowActions: [
            <IGridActionsRowButton>{
                label: "Edit on page",
                handler: (order) =>
                    this.router.navigateByUrl(`/app/demo/id-hashing/details/${order.id}`),
                materialIcon: "edit"
            },
            <IGridActionsRowButton>{
                handler: (order) => this.showInfo(this.service.getOrderInfo(order)),
                materialIcon: "info",
                buttonClass: "p-button-rounded p-button-text p-button-icon-only"
            }
        ],
        actionsColumnWidth: "11rem",
        updateMode: UpdateMode.Inline,
        filtersRow: true,
        transformBeforeUpdate: this.transformBeforeSave.bind(this),
        transformBeforeCreate: this.transformBeforeSave.bind(this)
    };

    simpleTableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { header: "ID", field: "id" },
            { header: "Company Name", field: "customerCompanyName" },
            {
                header: "Paid",
                field: "isPaid",
                displayHandler: (isPaid: boolean) => (isPaid ? "Yes" : "No")
            }
        ]
    };
    simpleGridSettings: IGridSettings = {
        readonly: true,
        sortEnabled: false
    };
    info: Order;
    showDialog = false;

    private customerOptions: SelectItem<EntityId>[] = [];

    constructor(
        private service: IdHashingDemoService,
        private simpleService: SimpleOrdersService,
        private customersService: CustomerService,
        private router: Router
    ) {
        this.init();
    }

    async init(): Promise<void> {
        this.gridSettings.dataService = this.service;
        this.simpleGridSettings.dataService = this.simpleService;

        this.customerOptions = (await this.customersService.getAll()).map((customer) => ({
            label: customer.code,
            value: customer.id
        }));

        this.tableSettings = {
            columns: <IGridColumn[]>[
                {
                    field: "id",
                    header: "ID",
                    editable: false,
                    filterCellEnabled: false
                },
                {
                    field: "customerId",
                    header: "Customer Code",
                    cellEditingInputType: CellEditInputType.Dropdown,
                    dropdownOptions: this.customerOptions,
                    placeholder: "Select customer",
                    validators: [ new GridValidator(Validators.required) ],
                    filterSettings: {
                        filterType: FilterType.Numeric,
                        inputType: FilterInputType.Dropdown,
                        dropdownOptions: this.customerOptions
                    }
                },
                {
                    header: "Order Date",
                    field: "orderDate",
                    displayMode: DisplayMode.Date,
                    cellEditingInputType: CellEditInputType.Calendar,
                    filterCellEnabled: false
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
                    header: "Paid",
                    field: "isPaid",
                    sortable: false,
                    displayMode: DisplayMode.Conditional,
                    displayConditionalTrueValue: "Yes",
                    displayConditionalFalseValue: "No",
                    cellEditingInputType: CellEditInputType.Checkbox,
                    filterSettings: {
                        filterType: FilterType.Boolean,
                        inputType: FilterInputType.Checkbox,
                        ignoreIfConvertibleToFalse: true
                    }
                }
            ]
        };
    }

    showInfo(getOrderInfo: Promise<Order>): void {
        getOrderInfo.then((data) => {
            this.info = data;
            this.showDialog = true;
        });
    }

    transformBeforeSave(row: Order) {
        const item = this.customerOptions.filter((option) => option.value === row.customerId);
        if (item.length) {
            row.customerCode = item[0].label;
        }

        return row;
    }
}
