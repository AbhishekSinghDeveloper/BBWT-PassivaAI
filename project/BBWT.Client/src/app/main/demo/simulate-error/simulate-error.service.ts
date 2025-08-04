import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";


@Injectable({
    providedIn: "root"
})
export class SimulateErrorService extends BaseDataService {
    readonly url = "api/demo/simulate-error";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    simulateError(code: number): Promise<any> {
        return this.httpPost("", {code});
    }

    simulateBadRequest(dto: any): Promise<any> {
        return this.httpPost("bad-request", dto);
    }

    simulateException(): Promise<any> {
        return this.httpGet("exception");
    }
}