import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IEmployee } from "../models";
import { PagedCrudService } from "@features/grid";


@Injectable({
    providedIn: "root"
})
export class EmployeeService extends PagedCrudService<IEmployee> {
    readonly url = "api/demo/employee";
    readonly entityTitle = "Employee";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    protected get modelUtcDateFields(): string[] {
        return ["registrationDate"]; 
    }


    getPageByODataUrl(odataUrl: string) {
        return this.httpGetByUrl(odataUrl);
    }
}