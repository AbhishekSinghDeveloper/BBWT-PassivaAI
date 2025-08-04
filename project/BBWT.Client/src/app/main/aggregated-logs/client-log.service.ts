import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { ClientLog } from "./client-log";

@Injectable({
    providedIn: "root"
})
export class ClientLogService extends BaseDataService {
    readonly url = "api/client-log";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    save(errorLog: ClientLog): Promise<any> {
        return this.httpPost("", errorLog, this.noHandler);
    }
}
