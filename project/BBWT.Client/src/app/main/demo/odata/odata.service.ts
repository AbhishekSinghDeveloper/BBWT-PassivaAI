import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { HttpClient } from "@angular/common/http";

@Injectable({
    providedIn: "root"
})
export class ODataService extends BaseDataService {
    readonly url = "";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    request(query: string): Promise<any> {
        return this.httpGetByUrl(query);
    }

    getPageByODataUrl(odataUrl: string): Promise<any> {
        return this.httpGetByUrl(odataUrl);
    }
}