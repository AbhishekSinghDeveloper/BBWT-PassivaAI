import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { Order } from "../models";


@Injectable()
export class IdHashingDemoService extends PagedCrudService<Order> {
    readonly url = "api/demo/id-hashing";
    readonly entityTitle = "Order";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    protected get modelUtcDateFields(): string[] {
        return ["orderDate", "requiredDate", "shippedDate"];
    }

    getOrderInfo(order: any): Promise<Order> {
        return this.httpGet(`info/${order.id}`);
    }
}
