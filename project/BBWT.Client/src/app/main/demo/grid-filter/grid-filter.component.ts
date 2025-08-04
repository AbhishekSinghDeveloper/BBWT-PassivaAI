import { Component } from "@angular/core";

import { FilterMatchMode, SelectItem } from "primeng/api";

import { CustomerService, OrderService } from "@demo/northwind";
import {
    CountableFilterMatchMode,
    FilterInputType,
    FilterType,
    IFilterSettings,
    StringFilterMatchMode
} from "@features/filter";
import {
    CreateMode,
    DisplayMode,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";


@Component({
    selector: "grid-filter",
    templateUrl: "./grid-filter.component.html"
})
export class GridFilterComponent {
    filterSettings: IFilterSettings[];
    tableSettings = {
        stateStorage: "session",
        stateKey: "demo-external-filters-grid",
        columns: <IGridColumn[]>[
            {
                field: "customerCode",
                header: "Customer Code",
                serverFieldName: "customer.code"
            },
            {
                field: "customer.companyName",
                header: "Customer Company Name"
            },
            {
                field: "orderDate",
                header: "Order Date",
                displayMode: DisplayMode.Date
            },
            {
                field: "requiredDate",
                header: "Required Date",
                displayMode: DisplayMode.Date
            },
            {
                field: "shippedDate",
                header: "Shipped Date",
                displayMode: DisplayMode.Date
            },
            {
                field: "isPaid",
                header: "Is Paid",
                displayMode: DisplayMode.Conditional,
                falseConditionValue: "No",
                trueConditionValue: "Yes",
                sortable: false
            },
            {
                field: "hasResellerItems",
                header: "Has Reseller Items",
                displayMode: DisplayMode.Conditional,
                falseConditionValue: "No",
                trueConditionValue: "Yes",
                sortable: false
            }
        ]
    } as ITableSettings;
    gridSettings = {
        readonly: true
    } as IGridSettings;
    tableWithNestedFiltersSettings: ITableSettings;
    gridWithNestedFiltersSettings = {
        readonly: true,
        filtersRow: true
    } as IGridSettings;
    isPaidOptions = <SelectItem[]>[
        { label: "All", value: null },
        { label: "Yes", value: true },
        { label: "No", value: false }
    ];


    constructor(private orderService: OrderService, private customerService: CustomerService) {
        this.init();
    }


    private async init(): Promise<void> {
        this.gridSettings.dataService = this.orderService;
        this.gridWithNestedFiltersSettings.dataService = this.orderService;

        const companyOptions = (await this.customerService.getAllCompanies()).map(x =>
            <SelectItem>{ label: x, value: x });

        this.filterSettings = <IFilterSettings[]>[
            {
                valueFieldName: "customer.code",
                header: "Customer code"
            },
            {
                valueFieldName: "customer.companyName",
                header: "Customer company name",
                inputType: FilterInputType.Dropdown,
                dropdownOptions: companyOptions
            },
            {
                valueFieldName: "orderDate",
                header: "Order date",
                filterType: FilterType.Date,
                inputType: FilterInputType.Calendar,
                matchMode: CountableFilterMatchMode.Equals,
                matchModeSelectorVisible: false
            },
            {
                valueFieldName: "requiredDate",
                header: "Required date",
                filterType: FilterType.Date,
                inputType: FilterInputType.Calendar,
                matchModeOptions: <SelectItem[]>[
                    { label: "Before", value: CountableFilterMatchMode.LessThan },
                    { label: "After", value: CountableFilterMatchMode.GreaterThan }
                ]
            },
            {
                valueFieldName: "shippedDate",
                header: "Shipped date",
                filterType: FilterType.Date,
                inputType: FilterInputType.Calendar,
                matchMode: CountableFilterMatchMode.Between
            },
            {
                header: "Is paid",
                valueFieldName: "isPaid"
            },
            {
                header: "Show both orders with and without reseller items",
                inputType: FilterInputType.Checkbox,
                valueFieldName: "hasResellerItems",
                filterType: FilterType.Boolean,
                ignoreIfConvertibleToTrue: true,
                defaultValue: true
            }
        ];
        this.tableWithNestedFiltersSettings = {
            stateStorage: "session",
            stateKey: "demo-nested-filters-grid",
            columns: <IGridColumn[]>[
                {
                    field: "customerCode",
                    header: "Customer Code",
                    serverFieldName: "customer.code"
                },
                {
                    field: "customer.companyName",
                    header: "Customer Company Name",
                    filterSettings: {
                        inputType: FilterInputType.Dropdown,
                        dropdownOptions: companyOptions
                    }
                },
                {
                    field: "orderDate",
                    header: "Order Date",
                    displayMode: DisplayMode.Date,
                    filterSettings: {
                        filterType: FilterType.Date,
                        inputType: FilterInputType.Calendar,
                        matchModeSelectorVisible: false,
                        matchMode: FilterMatchMode.IS
                    }
                },
                {
                    field: "requiredDate",
                    header: "Required Date",
                    displayMode: DisplayMode.Date,
                    filterSettings: {
                        filterType: FilterType.Date,
                        inputType: FilterInputType.Calendar,
                        matchModeOptions: <SelectItem[]>[
                            { label: "Before", value: FilterMatchMode.BEFORE },
                            { label: "After", value: FilterMatchMode.AFTER }
                        ]
                    }
                },
                {
                    field: "shippedDate",
                    header: "Shipped Date",
                    displayMode: DisplayMode.Date,
                    filterSettings: {
                        filterType: FilterType.Date,
                        inputType: FilterInputType.Calendar,
                        matchMode: FilterMatchMode.BETWEEN
                    }
                },
                {
                    field: "isPaid",
                    header: "Is Paid",
                    displayMode: DisplayMode.Conditional,
                    falseConditionValue: "No",
                    trueConditionValue: "Yes",
                    sortable: false,
                    filterSettings: {
                        matchModeSelectorVisible: false,
                        matchMode: FilterMatchMode.EQUALS
                    }
                },
                {
                    field: "hasResellerItems",
                    header: "Has Reseller Items",
                    displayMode: DisplayMode.Conditional,
                    falseConditionValue: "No",
                    trueConditionValue: "Yes",
                    sortable: false,
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
