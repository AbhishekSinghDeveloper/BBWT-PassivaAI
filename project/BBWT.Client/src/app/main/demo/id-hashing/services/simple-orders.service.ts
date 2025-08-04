import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedReaderService } from "@features/grid";
import { SimpleOrder } from "../models";


@Injectable()
export class SimpleOrdersService extends PagedReaderService<SimpleOrder> {
    readonly url = "api/demo/id-hashing/simple";
    readonly entityTitle = "Order (Simplified)";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}
