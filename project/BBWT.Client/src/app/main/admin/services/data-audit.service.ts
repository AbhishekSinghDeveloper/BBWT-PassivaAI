import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IDataAudit } from "../interfaces/data-audit";
import { PagedCrudService } from "@features/grid";


@Injectable()
export class DataAuditService extends PagedCrudService<IDataAudit> {
    readonly url = "api/data-audit";
    readonly entityTitle = "Data Audit";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}