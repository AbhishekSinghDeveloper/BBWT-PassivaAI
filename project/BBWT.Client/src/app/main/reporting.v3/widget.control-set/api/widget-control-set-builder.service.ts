import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IControlSetView} from "../widget-control-set.models";


@Injectable()
export class WidgetControlSetBuilderService extends BaseDataService {
    readonly url = "api/reporting3/widget/control-set/builder";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getView(widgetSourceId: string): Promise<IControlSetView> {
        return this.httpGet(`${widgetSourceId}/view`);
    }

    create(controlSetView: IControlSetView): Promise<IControlSetView> {
        return this.httpPost(null, controlSetView);
    }

    createDraft(controlSetView: IControlSetView, widgetSourceReleaseId?: string): Promise<IControlSetView> {
        return this.httpPost(`create-draft${widgetSourceReleaseId ? "/" + widgetSourceReleaseId : ""}`, controlSetView);
    }

    releaseDraft(widgetSourceDraftId: string): Promise<string> {
        return this.httpPut(`release-draft/${widgetSourceDraftId}`);
    }

    update(widgetSourceId: string, controlSetView: IControlSetView): Promise<IControlSetView> {
        return this.httpPut(`${widgetSourceId}`, controlSetView);
    }
}