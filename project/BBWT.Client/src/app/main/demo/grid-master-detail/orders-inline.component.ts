import { Component, ViewChild } from "@angular/core";

import { MessageService } from "primeng/api";

import { Message } from "@bbwt/classes";
import { IOrder, OrderService } from "@demo/northwind";
import { InlineDatatableCanDeactivate } from "@bbwt/guards/inline-datatable-can-deactivate";
import {
    CellEditInputType,
    CreateMode, DisplayMode, GridColumnViewSettings, GridComponent, GridExportDataRow, GridHelper,
    IGridColumn, IGridExportData,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";


@Component({
    selector: "orders",
    templateUrl: "./orders-inline.component.html",
    styleUrls: ["./orders-inline.component.scss"]
})
export class OrdersInlineComponent extends InlineDatatableCanDeactivate {
    selection: IOrder;
    tableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "customerCode",
                header: "Customer Code",
                serverFieldName: "customer.code",
                editable: false,
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                field: "orderDate",
                header: "Order Date",
                displayMode: DisplayMode.Date,
                cellEditingInputType: CellEditInputType.Calendar
            },
            {
                field: "requiredDate",
                header: "Required Date",
                displayMode: DisplayMode.Date,
                exportable: false,
                cellEditingInputType: CellEditInputType.Calendar
            },
            {
                field: "customColumn",
                header: "Custom Column Template",
                editable: false,
                sortable: false,
                exportable: false,
                displayHandler: (cellValue, rowData: IOrder) => `Click me - ${rowData.customerCode}` // Required for export!
            }
        ],
        selectionMode: "single",
    } as ITableSettings;
    expansionTableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "productTitle",
                header: "Product Title"
            },
            {
                field: "price",
                header: "Price",
                displayMode: DisplayMode.Number,
                numericInputMaxFractionDigits: 2,
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                field: "quantity",
                header: "Quantity",
                displayMode: DisplayMode.Number,
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            },
            {
                field: "isReseller",
                header: "Is Reseller",
                displayMode: DisplayMode.Conditional,
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            }
        ],
        selectionMode: "single",
        lazy: false
    } as ITableSettings;
    gridSettings = {
        createMode: CreateMode.External,
        updateMode: UpdateMode.Inline,
        deleteAllEnabled: true,
        exportEnabled: true,
        createFunc: () => this.addOrder(),
        selectColumn: true,
        rowExpansionEnabled: true,
        rowExpandableFunc: rowData => !!rowData.orderDetails?.length
    } as IGridSettings;
    expansionGridSettings = {
        readonly: true
    } as IGridSettings;
    @ViewChild(GridComponent, { static: true }) grid: GridComponent;


    constructor(public orderService: OrderService,
                private messageService: MessageService) {
        super();

        this.gridSettings.dataService = orderService;
        this.gridSettings.transformExportData = async (exportData: IGridExportData): Promise<IGridExportData> => {
            const extraExportColumns = (<IGridColumn[]> this.expansionTableSettings.columns).filter(x =>
                ["productTitle", "price", "quantity"].some(y => x.field === y));
            const newExportData: IGridExportData = {
                columns: [ ...exportData.columns, ...extraExportColumns ],
                data: []
            };

            exportData.data.forEach(oldExportDataRow => {
                if (!oldExportDataRow.rowData.orderDetails?.length) {
                    newExportData.data.push(oldExportDataRow);
                    return;
                }

                oldExportDataRow.rowData.orderDetails.forEach(orderDetailsItem => {
                    const newExportDataRow: GridExportDataRow = {
                        rowData: oldExportDataRow.rowData,
                        rowOutput: {...oldExportDataRow.rowOutput}
                    };

                    extraExportColumns.forEach(expansionTableColumn =>
                        newExportDataRow.rowOutput[expansionTableColumn.field] =
                            GridHelper.getCellDisplayValue(orderDetailsItem, expansionTableColumn));

                    newExportData.data.push(newExportDataRow);
                });
            });

            return newExportData;
        };
    }


    onGenerationCompleted(): void {
        this.messageService.add(Message.Success("Generation successful"));
        this.grid.reload();
    }

    private addOrder() {
        this.orderService.create(<IOrder> { customerCode: `${this.grid.getTableProperty("totalRecords")}-tmp` }).then(data => {
            this.messageService.add(Message.Info("New  order: " + data.id, "Inline editing data"));
            this.grid.reload();
        });
    }
}