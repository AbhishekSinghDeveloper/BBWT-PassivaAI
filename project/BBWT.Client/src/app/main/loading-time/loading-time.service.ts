import { HttpClient } from "@angular/common/http";
import { Injectable, EventEmitter, Output } from "@angular/core";

import { HttpResponsesHandlersFactory, IHttpResponseHandlerSettings } from "@bbwt/modules/data-service";
import { LoadingTime } from "./loading-time";
import { IQueryCommand } from "@features/filter";
import { PagedCrudService } from "@features/grid";


@Injectable()
export class LoadingTimeService extends PagedCrudService<LoadingTime> {
    readonly url = "api/loading-time";
    readonly entityTitle = "LoadingTime";

    // eslint-disable-next-line @angular-eslint/no-output-native
    @Output() change: EventEmitter<boolean> = new EventEmitter();


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    eventLoadingTime(): void {
        this.change.emit(true);
    }

    initalizeDate(): void {
        this.change.emit(false);
    }

    getAverage(filter: IQueryCommand): Promise<any> {
        return  this.handle<LoadingTime>(
            this.http.get<LoadingTime>(this.url + "/get-average", { params: this.constructHttpParams(filter) }),
            this.handlersFactory.getForReadByFilter());
    }

    create(item: LoadingTime): Promise<LoadingTime> {
        return this.httpPost("", item, this.noHandler);
    }
}