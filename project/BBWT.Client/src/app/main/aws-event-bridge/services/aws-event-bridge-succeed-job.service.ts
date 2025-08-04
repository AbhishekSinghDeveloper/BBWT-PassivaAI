import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IQueryCommand, NumberFilter } from "@features/filter";
import { PagedReaderService } from "@features/grid";
import { AwsEventBridgeJobHistory, JobCompletionStatus } from "../dto";


@Injectable()
export class AwsEventBridgeSucceedJobService extends PagedReaderService<AwsEventBridgeJobHistory> {
    readonly url = "api/aws-event-bridge-history";
    readonly entityTitle = "Event Bridge Succeed Job";

    constructor(httpClient: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(httpClient, handlersFactory);
    }

    extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(
            new NumberFilter("CompletionStatus", JobCompletionStatus.Succeed)
        );
    }
}
