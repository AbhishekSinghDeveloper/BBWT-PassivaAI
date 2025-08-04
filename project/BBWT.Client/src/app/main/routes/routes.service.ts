import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { IPageRoles } from "./page-roles";
import { IRouteRoles } from "./route-roles";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";


@Injectable({
    providedIn: "root"
})
export class RoutesService extends BaseDataService {
    readonly url = "api/route";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getApiRouteRoles(): Promise<IRouteRoles[]> {
        return this.httpGet("api-routes-roles");
    }

    getPageRoles(): Promise<IPageRoles[]> {
        return this.httpGet("pages-roles");
    }

    getCurrentUserAllowedRoutesPaths(): Promise<string[]> {
        return this.httpGet("me");
    }
}