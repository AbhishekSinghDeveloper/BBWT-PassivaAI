import { Component } from "@angular/core";

import { SecurityAccessibleService } from "./security-accessible.service";
import { DisplayMode, IGridColumn, IGridSettings, ITableSettings } from "@features/grid";
import { FilterInputType, FilterType } from "@features/filter";


@Component({
    selector: "demo-security-any_user",
    templateUrl: "./accessible-to-any-authenticated.component.html"
})
export class AccessibleToAnyAuthenticatedComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "customerCode",
                header: "Customer",
                serverFieldName: "customer.code"
            },
            {
                field: "customerCompanyName",
                header: "Company",
                serverFieldName: "customer.companyName"
            },
            {
                field: "isPaid",
                sortable: false,
                displayMode: DisplayMode.Conditional,
                displayConditionalTrueValue: "Yes",
                displayConditionalFalseValue: "No",
                header: "Is Paid",
                filterSettings: {
                    filterType: FilterType.Boolean,
                    inputType: FilterInputType.Checkbox,
                    ignoreIfConvertibleToFalse: true
                }
            },
            {
                field: "orderDate",
                header: "Order Date",
                displayMode: DisplayMode.Date,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            {
                field: "shippedDate",
                header: "Shipped Date",
                displayMode: DisplayMode.Date,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            }
        ]
    };
    gridSettings: IGridSettings = {
        readonly: true,
        filtersRow: true,
        dataServiceGetPageMethodName: "getDataForAccessibleToAnyAuthenticated"
    };

    constructor(private service: SecurityAccessibleService) {
        this.gridSettings.dataService = service;
    }
}