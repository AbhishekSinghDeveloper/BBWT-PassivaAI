import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { AllowedIp } from "./allowed-ip-models";
import { PagedCrudService } from "@features/grid";

@Injectable()
export class AllowedIpService extends PagedCrudService<AllowedIp> {
    readonly entityTitle = "Allowed Ip";
    readonly url = "api/allowed-ip";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}