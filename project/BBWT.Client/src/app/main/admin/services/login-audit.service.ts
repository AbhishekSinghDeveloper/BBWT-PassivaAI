import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { ILoginAudit } from "../interfaces/login-audit";
import { PagedCrudService } from "@features/grid";


@Injectable()
export class LoginAuditService extends PagedCrudService<ILoginAudit> {
    readonly url = "api/login-audit";
    readonly entityTitle = "Login Audit";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}