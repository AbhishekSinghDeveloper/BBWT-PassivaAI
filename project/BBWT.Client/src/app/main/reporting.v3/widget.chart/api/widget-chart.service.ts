import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IChartBuildDTO, IChartViewDTO} from "@main/reporting.v3/widget.chart/widget-chart.models";
import {IQueryVariables} from "@main/reporting.v3/core/variables/variable-models";
import {IViewMetadata} from "@main/reporting.v3/core/reporting-models";

@Injectable({
    providedIn: "root"
})
export class WidgetChartService extends BaseDataService {
    readonly url = "api/reporting3/widget/chart";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getView(widgetSourceId: string): Promise<IChartViewDTO> {
        return this.httpGet(`${widgetSourceId}/view`);
    }

    create(chart: IChartBuildDTO): Promise<IChartBuildDTO> {
        return this.httpPost(null, chart);
    }

    createDraft(chart: IChartBuildDTO, widgetSourceReleaseId?: string): Promise<IChartBuildDTO> {
        return this.httpPost(`create-draft${widgetSourceReleaseId ? "/" + widgetSourceReleaseId : ""}`, chart);
    }

    releaseDraft(widgetSourceDraftId: string): Promise<string> {
        return this.httpPut(`release-draft/${widgetSourceDraftId}`);
    }

    update(id: string, chart: IChartBuildDTO): Promise<IChartBuildDTO> {
        return this.httpPut(`${id}`, chart);
    }

    getQuerySchema(querySourceId: string): Promise<any> {
        return this.httpGet(`${querySourceId}/query-schema`);
    }

    getViewMetadata(querySourceId: string): Promise<IViewMetadata> {
        return this.httpGet(`${querySourceId}/view-metadata`);
    }

    getQueryDataRows(querySourceId: string, queryVariables?: IQueryVariables): Promise<any[]> {
        return this.handle(this.http.get<any[]>(`${this.url}/${querySourceId}/query-data`,
            {params: this.constructHttpParams(queryVariables ?? {variables: []})}));
    }
}