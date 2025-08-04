import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { from, Observable } from "rxjs";
import { AwsEventBridgeJobInfo, AwsEventBridgeRule } from "../dto";

@Injectable()
export class AwsEventBridgeRuleService extends PagedCrudService<AwsEventBridgeRule> {
    readonly url = "api/aws-event-bridge-rule";
    readonly entityTitle = "Event Bridge Rule";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    ruleExists(name: string): Observable<boolean> {
        return from(this.httpGet<boolean>(`exists/${name}`));
    }
}
