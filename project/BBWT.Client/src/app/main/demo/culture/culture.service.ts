import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";


@Injectable()
export class CultureService extends BaseDataService {
    readonly url = "api/demo/culture";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    getCulture(data): Promise<any> {
        return this.httpPost("", data);
    }
}