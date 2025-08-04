import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { HttpResponsesHandlersFactory, IHttpResponseHandlerSettings } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { Parameter } from "./classes/parameter";


@Injectable()
export class ParametersStoreService extends PagedCrudService<Parameter> {
    readonly url = "api/app-settings";
    readonly entityTitle = "Application Parameter";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    create(item: Parameter, handlerSettings?: IHttpResponseHandlerSettings): Promise<Parameter> {
        return this.httpPost("", item, this.handlersFactory.getForCreate(this.entityTitle, handlerSettings))
            .then(() => item);
    }

    update(id: string, item: Parameter, handlerSettings?: IHttpResponseHandlerSettings): Promise<Parameter> {
        return this.httpPost("", item, this.handlersFactory.getForUpdate(this.entityTitle, handlerSettings))
            .then(() => item);
    }

    getAppSettings(): Promise<Parameter[]> {
        return this.httpGet("", this.handlersFactory.getForReadAll());
    }

    isEnabled(): Promise<boolean> {
        return this.httpGet("is-enabled");
    }
}