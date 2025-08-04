import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { AwsEventBridgeJobInfo } from "../dto";

@Injectable()
export class AwsEventBridgeJobService extends BaseDataService {
    readonly url = "api/aws-event-bridge-job";

    constructor(httpClient: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(httpClient, handlersFactory);
    }

    getJobs(): Promise<AwsEventBridgeJobInfo[]> {
        return this.httpGet("jobs-list");
    }

    restartJob(historyId: number): Promise<any> {
        return this.httpPost(`restart-job/${historyId}`);
    }
}
