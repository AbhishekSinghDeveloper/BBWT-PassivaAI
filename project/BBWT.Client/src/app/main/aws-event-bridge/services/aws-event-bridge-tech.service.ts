import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { ClearTablesResult } from "../dto";

@Injectable()
export class AwsEventBridgeTechService extends BaseDataService {
    readonly url = "api/aws-event-bridge-tech";

    constructor(httpClient: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(httpClient, handlersFactory);
    }

    clearTables(): Promise<ClearTablesResult> {
        return this.httpPut("clear-tables", null);
    }
}
