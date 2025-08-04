import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IPermission } from "./permission";


@Injectable()
export class PermissionService extends BaseDataService {
    readonly url = "api/permission";
    readonly entityTitle = "Permission";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getAll(): Promise<IPermission[]> {
        return this.httpGet();
    }
}