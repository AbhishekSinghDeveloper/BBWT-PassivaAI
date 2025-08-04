import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedReaderService } from "@features/grid";
import { AwsEventBridgeRunningJob } from "../dto";

@Injectable()
export class AwsEventBridgeRunningJobService extends PagedReaderService<AwsEventBridgeRunningJob> {
    readonly url = "api/aws-event-bridge-running";
    readonly entityTitle = "Event Bridge Running Job";

    constructor(httpClient: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(httpClient, handlersFactory);
    }

    cancelJob(cancelationId: string): Promise<any> {
        return this.httpPut(`cancel/${cancelationId}`);
    }
}
