import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import {
    HttpResponsesHandlersFactory,
    IHttpResponseHandlerSettings
} from "@bbwt/modules/data-service";
import { IQueryCommand } from "@features/filter";
import { IPagedData } from "@features/grid";
import { PagedReaderService } from "@features/grid";
import { AwsEventBridgeJobHistory } from "../dto";

@Injectable()
export class AwsEventBridgeCanceledJobService extends PagedReaderService<AwsEventBridgeJobHistory> {
    readonly url = "api/aws-event-bridge-history";
    readonly entityTitle = "Event Bridge Succeed Job";

    constructor(httpClient: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(httpClient, handlersFactory);
    }

    getPage(
        queryCommand: IQueryCommand,
        responseHandlerSettings?: IHttpResponseHandlerSettings
    ): Promise<IPagedData<AwsEventBridgeJobHistory>> {
        if (this.extendQueryCommand) {
            this.extendQueryCommand(queryCommand);
        }

        return this.handle<IPagedData<AwsEventBridgeJobHistory>>(
            this.http.get<IPagedData<AwsEventBridgeJobHistory>>(`${this.url}/canceled-jobs-page`, {
                params: this.constructHttpParams(queryCommand)
            }),
            this.handlersFactory.getForReadByFilter(responseHandlerSettings)
        );
    }
}
