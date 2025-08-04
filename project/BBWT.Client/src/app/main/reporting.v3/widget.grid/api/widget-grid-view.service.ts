import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IGridDisplayView } from "../widget-grid.models";


@Injectable({
    providedIn: "root"
})
export class WidgetGridViewService extends BaseDataService {
    readonly url = "api/reporting3/widget/grid/view";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getDisplayView(widgetSourceId: string): Promise<IGridDisplayView> {
        return this.httpGet(`${widgetSourceId}`);
    }
}