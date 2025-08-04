import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { IOrderDetails } from "../models/order-details";


@Injectable({
    providedIn: "root"
})
export class OrderDetailsService extends PagedCrudService<IOrderDetails> {
    readonly url = "api/demo/order-details";
    readonly entityTitle = "Order Details";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}