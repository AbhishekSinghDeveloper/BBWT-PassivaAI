import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IUser } from "@main/users";


@Injectable({
    providedIn: "root"
})
export class ImpersonationService extends BaseDataService {
    readonly url = "api/demo/impersonation";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    getImpersonatedDemoManager(): Promise<IUser> {
        return this.httpGet("impersonated-demo-manager");
    }

    getImpersonatedDemoUser(): Promise<IUser> {
        return this.httpGet("impersonated-demo-user");
    }
}