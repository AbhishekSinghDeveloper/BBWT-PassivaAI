import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { FormIOParameter } from "./classes/FormIOParameter";

@Injectable()
export class FormioParameterListService extends PagedCrudService<FormIOParameter> {
    public readonly url = "api/formio-parameters";
    public readonly entityTitle = "FormIO Parameter";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}