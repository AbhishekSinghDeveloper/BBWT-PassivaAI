import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IGridView} from "../widget-grid.models";


@Injectable()
export class WidgetGridBuilderService extends BaseDataService {
    readonly url = "api/reporting3/widget/grid/builder";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getView(widgetSourceId: string): Promise<IGridView> {
        return this.httpGet(`${widgetSourceId}/view`);
    }

    create(gridView: IGridView): Promise<IGridView> {
        return this.httpPost(null, gridView);
    }

    createDraft(grid: IGridView, widgetSourceReleaseId?: string): Promise<IGridView> {
        return this.httpPost(`create-draft${widgetSourceReleaseId ? "/" + widgetSourceReleaseId : ""}`, grid);
    }

    releaseDraft(widgetSourceDraftId: string): Promise<string> {
        return this.httpPut(`release-draft/${widgetSourceDraftId}`);
    }

    update(widgetSourceId: string, gridView: IGridView): Promise<IGridView> {
        return this.httpPut(`${widgetSourceId}`, gridView);
    }
}