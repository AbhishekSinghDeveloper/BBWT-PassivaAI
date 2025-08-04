import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { ReportProblem } from "./report-problem";
import { ErrorLog } from "./error-log";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { firstValueFrom } from "rxjs";

@Injectable({
    providedIn: "root"
})
export class ReportProblemService extends BaseDataService {
    readonly url = "api/report-problem";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    send(reportProblem: ReportProblem): Promise<any> {
        return this.httpPost("", reportProblem, this.noHandler);
    }

    autoSend(errorLog: ErrorLog): Promise<any> {
        return firstValueFrom(this.http.post(`${this.url}/auto-send`, errorLog));
    }
}
