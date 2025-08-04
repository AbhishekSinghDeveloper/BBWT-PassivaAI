import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IHtmlDTO, IHtmlViewDTO} from "@main/reporting.v3/widget.html/widget-html.models";

@Injectable({
    providedIn: "root"
})
export class WidgetHtmlService extends BaseDataService {
    readonly url = "api/reporting3/widget/html";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getView(widgetSourceId: string): Promise<IHtmlViewDTO> {
        return this.httpGet(`${widgetSourceId}/view`);
    }

    create(html: IHtmlDTO): Promise<IHtmlDTO> {
        return this.httpPost(null, html);
    }

    createDraft(html: IHtmlDTO, widgetSourceReleaseId?: string): Promise<IHtmlDTO> {
        return this.httpPost(`create-draft${widgetSourceReleaseId ? "/" + widgetSourceReleaseId : ""}`, html);
    }

    releaseDraft(widgetSourceDraftId: string): Promise<string> {
        return this.httpPut(`release-draft/${widgetSourceDraftId}`);
    }

    update(id: string, html: IHtmlDTO): Promise<IHtmlDTO> {
        return this.httpPut(`${id}`, html);
    }
}