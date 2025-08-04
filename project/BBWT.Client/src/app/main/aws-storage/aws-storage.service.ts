import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { AwsCheckPermissionsResult } from "./aws-check-permissions-result";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";


@Injectable()
export class AwsStorageService extends BaseDataService {
    readonly url = "api/aws-storage";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    checkPermissions(): Promise<AwsCheckPermissionsResult> {
        return this.httpGet("permissions-check");
    }
}