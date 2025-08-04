import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { HttpResponsesHandlersFactory } from "../../../bbwt/modules/data-service";
import { PagedCrudService } from "../../../features/grid";
import { INamedQuery } from "../../reporting/reporting-models";


@Injectable()
export class NamedQueryService extends PagedCrudService<INamedQuery> {
    readonly url = "api/named-query";
    readonly entityTitle = "Named Query";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}