import {Injectable} from "@angular/core";
import {HttpClient} from "@angular/common/http";

import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {PagedCrudService} from "@features/grid";
import {IWidgetSource, IWidgetSourcePreload} from "../core/reporting-models";


@Injectable()
export class WidgetSourceService extends PagedCrudService<IWidgetSource> {
    readonly url = "api/reporting3/widget-source";
    readonly entityTitle = "Widget Source";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    getWidgetSourcePreload(widgetCode: string, disableAuthorization: boolean = false): Promise<IWidgetSourcePreload> {
        return this.handle(this.http.get<IWidgetSourcePreload>(`${this.url}/${widgetCode}/preload`,
            {params: this.constructHttpParams({disableAuthorization: disableAuthorization})}));
    }

    publishWidget(widgetSourceId: string, organizationIds: number[], disableAuthorization: boolean = false): Promise<void> {
        return this.handle(this.http.put<void>(`${this.url}/${widgetSourceId}/publish`, organizationIds,
            {params: this.constructHttpParams({disableAuthorization: disableAuthorization})}));
    }

    changeOwner(widgetSourceId: string, ownerId: string, disableAuthorization: boolean = false): Promise<void> {
        return this.handle(this.http.put<void>(`${this.url}/${widgetSourceId}/change-owner`, null,
            {params: this.constructHttpParams({ownerId: ownerId, disableAuthorization: disableAuthorization})}));
    }
}