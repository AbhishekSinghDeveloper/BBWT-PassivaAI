import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IQueryDataRequest} from "@main/reporting.v3/core/reporting-models";
import {SelectItem} from "primeng/api";


@Injectable({
    providedIn: "root"
})
export class WidgetControlSetDataService extends BaseDataService {
    readonly url = "api/reporting3/widget/control-set/data";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getDropdownData(queryDataRequest: IQueryDataRequest): Promise<SelectItem[]> {
        return this.handle(this.http.get<SelectItem[]>(`${this.url}/dropdown-data`, {
            params: this.constructHttpParams(queryDataRequest)
        }));
    }
}