import {Injectable} from "@angular/core";
import {HttpClient} from "@angular/common/http";

import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {PagedCrudService} from "@features/grid";
import {IVariable} from "@main/reporting.v3/core/reporting-models";


@Injectable()
export class VariablesService extends PagedCrudService<IVariable> {
    readonly url = "api/reporting3/variables";
    readonly entityTitle = "Variable";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}