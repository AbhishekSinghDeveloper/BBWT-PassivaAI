import {Injectable} from "@angular/core";
import {HttpClient} from "@angular/common/http";

import {PagedCrudService} from "@features/grid";

import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {ICustomer} from "../models/customer";

@Injectable({
    providedIn: "root"
})
export class CustomerService extends PagedCrudService<ICustomer> {
    readonly url = "api/demo/customer";
    readonly entityTitle = "Customer";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getAllCompanies(): Promise<string[]> {
        return this.httpGet("all-companies");
    }
}