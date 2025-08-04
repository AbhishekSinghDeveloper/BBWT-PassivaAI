import { Component, ViewChild } from "@angular/core";

import { MessageService } from "primeng/api";

import {
    CellEditInputType,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    IGridColumn,
    IGridSettings,
    ITableSettings
} from "@features/grid";
import { IOrder, OrderService } from "@demo/northwind";
import { Message } from "@bbwt/classes";
import {
    DbDocService,
    getGridExternalMetadataFromTableMetadataResult
} from "@main/dbdoc";


@Component({
    selector: "orders-popup",
    templateUrl: "./orders-popup.component.html",
    styleUrls: ["./orders-popup.component.scss"]
})
export class OrdersPopupComponent {
    selection: IOrder[];
    tableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "customerCode",
                serverFieldName: "customer.code",
                header: "Customer Code",
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
                cellEditingInputType: CellEditInputType.Calendar
            }
        ],
        selectionMode: "multiple",
    } as ITableSettings;
    gridSettings = {
        deleteAllEnabled: true,
        exportEnabled: true,
        externalMetadata: () => {
            return this.dbDocService.getTableMetadata("DemoContext.Orders")
                .then(result => getGridExternalMetadataFromTableMetadataResult(result));
        },
        additionalRowActions: [
            {
                label: "Additional action",
                materialIcon: "flash_on",
                handler: row => {
                    this.shownOrder = row;
                    this.additionalActionDialogVisible = true;
                }
            }
        ],
        actionsColumnWidth: "13rem"
    } as IGridSettings;
    additionalActionDialogVisible = false;
    shownOrder: IOrder;
    @ViewChild(GridComponent, { static: true }) grid: GridComponent;


    constructor(public orderService: OrderService,
                private messageService: MessageService,
                private dbDocService: DbDocService) {
        this.gridSettings.dataService = orderService;
    }


    onGenerationCompleted(): void {
        this.messageService.add(Message.Success("Generation successful"));
        this.grid.reload();
    }
}