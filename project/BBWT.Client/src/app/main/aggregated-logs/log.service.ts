import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { ILog } from "./log";

@Injectable()
export class LogService extends PagedCrudService<ILog> {
    readonly url = "api/log";
    public readonly entityTitle = "Logs";

    constructor(httpClient: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(httpClient, handlersFactory);
    }
}
