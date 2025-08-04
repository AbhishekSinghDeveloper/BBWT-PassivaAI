import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";


@Injectable({
    providedIn: "root"
})
export class ContextVariablesService extends BaseDataService {
    readonly url = "api/reporting3/query/context-variables";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getVariableNames(): Promise<string[]> {
        return this.httpGet(null);
    }
}