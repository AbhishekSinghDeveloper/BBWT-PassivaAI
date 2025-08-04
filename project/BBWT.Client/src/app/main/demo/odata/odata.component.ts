import { Component } from "@angular/core";
import { HttpErrorResponse } from "@angular/common/http";

import { SelectItem } from "primeng/api";

import { ODataService } from "./odata.service";
import { FilterInputType, FilterType } from "@features/filter";
import {
    DisplayMode,
    IGridColumn,
    IGridSettings,
    ITableSettings
} from "@features/grid";


enum QueryExampleName {
    Aggregate,
    ExpandSelect,
    FilteringPagingOrdering
}

@Component({
    templateUrl: "./odata.component.html",
    styleUrls: ["./odata.component.scss"]
})
export class ODataComponent {
    queryExampleOptions = <SelectItem[]>[
        { label: "Aggregate", value: QueryExampleName.Aggregate },
        { label: "Expand & Select", value: QueryExampleName.ExpandSelect },
        { label: "Filtering, paging and ordering", value: QueryExampleName.FilteringPagingOrdering }
    ];
    predefinedQuery = "";
    queryResult: string;
    errorResponseString: string;
    manuallyTypedQuery = "";

    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "id",
                header: "Id",
                filterSettings: {
                    filterType: FilterType.Numeric
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
                field: "requiredDate",
                header: "Required Date",
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
            },
            {
                field: "isPaid",
                header: "Is Paid",
                sortable: false,
                filterSettings: {
                    filterType: FilterType.Boolean,
                    inputType: FilterInputType.Checkbox,
                    ignoreIfConvertibleToFalse: true
                }
            },
            {
                field: "customer.code",
                header: "Customer Code"
            },
            {
                field: "orderDetails",
                header: "Order Details Count",
                expand: true,
                displayHandler: (cellValue: any[], rowValue) => String(cellValue.length),
                filterSettings: {
                    filterType: FilterType.Numeric,
                    inputType: FilterInputType.Number,
                    matchModeSelectorVisible: false
                }
            }
        ]
    };
    gridSettings: IGridSettings = {
        readonly: true,
        filtersRow: true,
        isODataGetRequest: true,
        oDataQueryTransform: query => {
            // Here you can correct the OData query as you wish.

            return query;
        },
        oDataUrl: "odata/Orders",
        dataServiceGetPageMethodName: "getPageByODataUrl"
    };

    get queryStringResult(): string {
        return this.queryResult ? JSON.stringify(this.queryResult, null, 4) : "";
    }


    constructor(public oDataService: ODataService) {
        this.gridSettings.dataService = oDataService;
    }


    selectQueryExample(value: QueryExampleName): void {
        switch (value) {
            case QueryExampleName.Aggregate:
                this.predefinedQuery = "odata/customers?$expand=Orders($apply=aggregate($count as Count))";
                break;
            case QueryExampleName.ExpandSelect:
                this.predefinedQuery = "odata/customers?$select=Code&$expand=Orders($select=Id)";
                break;
            case QueryExampleName.FilteringPagingOrdering:
                this.predefinedQuery = "odata/customers?$skip=5&$top=10&$orderby=Id desc&$filter=Id ge 50 and Contains(Code,'A')";
                break;
        }
    }

    sendPredefinedRequest(): void {
        this.oDataService.request(this.predefinedQuery).then(result => {
            this.queryResult = result;
            this.errorResponseString = null;
        }).catch(this.handleError);
    }

    sendManuallyTypedRequest(): void {
        this.oDataService.request(this.manuallyTypedQuery).then(result => {
            this.queryResult = result;
            this.errorResponseString = null;
        }).catch(this.handleError);
    }


    private handleError = (response: HttpErrorResponse): void => {
        this.queryResult = null;

        if (response.error.error) {
            this.errorResponseString = response.error.error.message;
            return;
        }
        if (response.message) {
            this.errorResponseString = response.message;
            return;
        }
    }
}