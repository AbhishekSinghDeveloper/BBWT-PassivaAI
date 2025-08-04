import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IQueryCommand, NumberFilter } from "@features/filter";
import { PagedReaderService } from "@features/grid";
import { AwsEventBridgeJobHistory } from "../dto/aws-event-bridge-job-history";
import { JobCompletionStatus } from "../dto";

@Injectable()
export class AwsEventBridgeFailedJobService extends PagedReaderService<AwsEventBridgeJobHistory> {
    readonly url = "api/aws-event-bridge-history";
    readonly entityTitle = "Event Bridge Failed Job";

    constructor(httpClient: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(httpClient, handlersFactory);
    }

    extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberFilter("CompletionStatus", JobCompletionStatus.Failed));
    }
}
