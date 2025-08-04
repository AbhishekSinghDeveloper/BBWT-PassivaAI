import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IQuerySchema, IViewMetadata, IQueryPageRequest, IQueryColumnAggregation} from "../../core/reporting-models";
import {IQueryVariables} from "@main/reporting.v3/core/variables/variable-models";


@Injectable({
    providedIn: "root"
})
export class WidgetGridDataService extends BaseDataService {
    readonly url = "api/reporting3/widget/grid/data";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getDataRows(querySourceId: string, queryPageRequest?: IQueryPageRequest): Promise<any[]> {
        return this.handle(this.http.get<any[]>(`${this.url}/${querySourceId}/query-data`, {
            params: this.constructHttpParams(queryPageRequest
                ?? <IQueryPageRequest>{gridSettings: {}, queryVariables: {variables: []}})
        }));
    }

    getDataRowsCount(querySourceId: string, queryVariables?: IQueryVariables): Promise<number> {
        return this.handle(this.http.get<number>(`${this.url}/${querySourceId}/query-data-count`, {
            params: this.constructHttpParams(queryVariables ?? <IQueryVariables>{variables: []})
        }));
    }

    getAggregations(querySourceId: string, aggregations: IQueryColumnAggregation[],
                    queryVariables?: IQueryVariables): Promise<any> {
        return this.handleRequest(
            this.http.get<any>(`${this.url}/${querySourceId}/query-data-aggregations`, {
                params: this.constructHttpParams({
                    queryVariables: queryVariables ?? <IQueryVariables>{variables: []},
                    aggregations: aggregations
                })
            }));
    }

    getQuerySchema(querySourceId: string): Promise<IQuerySchema> {
        return this.handle(this.http.get<IQuerySchema>(`${this.url}/${querySourceId}/query-schema`));
    }

    getViewMetadata(widgetSourceId: string): Promise<IViewMetadata> {
        return this.httpGet(`${widgetSourceId}/view-metadata`);
    }
}