import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { SystemSummary } from "./system-summary";
import { HttpResponsesHandlersFactory, BaseDataService, IHttpResponseHandlerSettings } from "../data-service";
import { Version } from "./version";

@Injectable({
    providedIn: "root"
})
export class SystemDataService extends BaseDataService {
    url = "api/system-data";
    systemSummary: Promise<SystemSummary>;

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getSystemSummary(): Promise<SystemSummary> {
        if (!this.systemSummary) {
            this.systemSummary = this.httpGet<SystemSummary>("summary");
        }
        return this.systemSummary;
    }

    getVersionInfo(handlerSettings?: IHttpResponseHandlerSettings): Promise<Version> {
        return this.httpGet<Version>("version", this.defaultHandler(handlerSettings));
    }

    getDebugData() {
        return this.httpGet("debug");
    }

    getApiExceptionsData() {
        return this.httpGet("debug/api-exeptions");
    }

    //TODO: related to refresh feature which is in development
    renew() {
        return this.httpGet("renew", this.noHandler);
    }
}