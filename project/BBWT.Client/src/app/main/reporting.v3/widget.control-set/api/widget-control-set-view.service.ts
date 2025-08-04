import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IControlSetDisplayView } from "../widget-control-set.models";


@Injectable({
    providedIn: "root"
})
export class WidgetControlSetViewService extends BaseDataService {
    readonly url = "api/reporting3/widget/control-set/view";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    getDisplayView(widgetSourceId: string): Promise<IControlSetDisplayView> {
        return this.httpGet(`${widgetSourceId}`);
    }
}