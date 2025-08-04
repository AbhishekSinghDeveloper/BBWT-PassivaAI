import {Injectable} from "@angular/core";
import {HttpClient} from "@angular/common/http";

import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {PagedCrudService} from "@features/grid";
import {IPagedGridSettings, IQuerySchema, IQuerySource} from "../core/reporting-models";


@Injectable()
export class QuerySourceService extends PagedCrudService<IQuerySource> {
    readonly url = "api/reporting3/query-source";
    readonly entityTitle = "Query Source";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    getQuerySchema(querySourceId: string, disableAuthorization: boolean = false): Promise<IQuerySchema> {
        return this.handle(this.http.get<IQuerySchema>(`${this.url}/${querySourceId}/query-schema`,
            {params: this.constructHttpParams({disableAuthorization: disableAuthorization})}));
    }

    getQueryDataRows(querySourceId: string, gridSettings: IPagedGridSettings, disableAuthorization: boolean = false): Promise<any[]> {
        return this.handle(this.http.get<any[]>(`${this.url}/${querySourceId}/query-data-rows`,
            {params: this.constructHttpParams({...gridSettings, disableAuthorization: disableAuthorization})}));
    }

    getQueryDataRowsCount(querySourceId: string, disableAuthorization: boolean = false): Promise<number> {
        return this.handle(this.http.get<number>(`${this.url}/${querySourceId}/query-data-rows-count`,
            {params: this.constructHttpParams({disableAuthorization: disableAuthorization})}));
    }

    hasAttachedWidgets(querySourceId: string, disableAuthorization: boolean = false): Promise<boolean> {
        return this.handle(this.http.get<boolean>(`${this.url}/${querySourceId}/has-attached-widgets`,
            {params: this.constructHttpParams({disableAuthorization: disableAuthorization})}));
    }

    publishQuery(querySourceId: string, organizationIds: number[], disableAuthorization: boolean = false): Promise<void> {
        return this.handle(this.http.put<void>(`${this.url}/${querySourceId}/publish`, organizationIds,
            {params: this.constructHttpParams({disableAuthorization: disableAuthorization})}));
    }

    changeOwner(querySourceId: string, ownerId: string, disableAuthorization: boolean = false): Promise<void> {
        return this.handle(this.http.put<void>(`${this.url}/${querySourceId}/change-owner`, null,
            {params: this.constructHttpParams({ownerId: ownerId, disableAuthorization: disableAuthorization})}));
    }
}