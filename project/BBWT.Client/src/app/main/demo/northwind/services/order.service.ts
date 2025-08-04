import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { IOrder } from "../models";


@Injectable({
    providedIn: "root"
})
export class OrderService extends PagedCrudService<IOrder> {
    readonly url = "api/demo/order";
    readonly entityTitle = "Order";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    protected get modelUtcDateFields(): string[] {
        return ["orderDate", "requiredDate", "shippedDate"];
    }
}