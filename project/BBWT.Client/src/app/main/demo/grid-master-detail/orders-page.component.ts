import { Component, ViewChild } from "@angular/core";

import { MessageService, SelectItem } from "primeng/api";

import { OrderService, IOrder} from "@demo/northwind";
import { Message } from "@bbwt/classes";
import {
    CellEditInputType, CreateMode,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    GridValidator,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { notEmptyValidator } from "@bbwt/modules/validation";


@Component({
    selector: "orders",
    templateUrl: "./orders-page.component.html",
    styleUrls: ["./orders-page.component.scss"]
})
export class OrdersPageComponent {
    selection: IOrder[];
    tableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "customerCode",
                serverFieldName: "customer.code",
                header: "Customer Code",
                viewSettings: new GridColumnViewSettings({ width: "150px" })
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
            }
        ],
        selectionMode: "multiple",
        stateStorage: "session",
        stateKey: "orders-page-grid"
    } as ITableSettings;
    gridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/demo/grid-master-detail/create",
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/demo/grid-master-detail/edit/:id",
        deleteAllEnabled: true,
        exportEnabled: true,
        selectColumn: true
    } as IGridSettings;
    @ViewChild(GridComponent, { static: true }) grid: GridComponent;


    constructor(public orderService: OrderService,
                private messageService: MessageService) {
        this.gridSettings.dataService = orderService;
    }

    onGenerationCompleted(): void {
        this.messageService.add(Message.Success("Generation successful"));
        this.grid.reload();
    }
}