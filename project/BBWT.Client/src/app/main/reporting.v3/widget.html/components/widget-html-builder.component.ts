import {Component, EventEmitter, Input, Output} from "@angular/core";
import {Mode} from "highlight.js";
import {Message} from "@bbwt/classes";
import {MessageService} from "primeng/api";
import {IHtmlDTO, IHtmlViewDTO} from "@main/reporting.v3/widget.html/widget-html.models";
import {WidgetHtmlService} from "@main/reporting.v3/widget.html/api/widget-html.service";
import {IWidgetSource} from "@main/reporting.v3/core/reporting-models";
import {IWidgetBuilder} from "@main/reporting.v3/core/widget-builder";


@Component({
    selector: "widget-html-builder",
    templateUrl: "./widget-html-builder.component.html",
    styleUrls: ["./widget-html-builder.component.scss"]
})
export class WidgetHtmlBuilderComponent implements IWidgetBuilder {
    // General settings.
    loading: boolean;

    // Editor settings.
    htmlCode: string;
    toolbar: string = "all";
    language: string = "html";
    tokens: Mode[] = [{
        scope: "variable",
        match: /#[a-zA-Z0-9_]+/,
        relevance: 1
    }];

    // Html preview.
    htmlPreview: IHtmlViewDTO;

    private _htmlView: IHtmlViewDTO;
    private _widgetSource: IWidgetSource = {widgetType: "html"} as IWidgetSource;

    @Output() widgetSourceChange: EventEmitter<IWidgetSource> = new EventEmitter<IWidgetSource>();

    constructor(private widgetHtmlService: WidgetHtmlService,
                private messageService: MessageService) {
    }

    // Html view for updating purposes.
    @Input() set htmlView(value: IHtmlViewDTO) {
        this._htmlView = value ?? {widgetSource: {widgetType: "html"}} as IHtmlViewDTO;
        this._widgetSource = this._htmlView.widgetSource;
        this.widgetSourceChange.emit(this._widgetSource);
        this.refreshWidget().then();
    }

    get htmlView() {
        return this._htmlView;
    }

    // Widget source id for updating purposes.
    @Input() set widgetSourceId(value: string) {
        if (value === this.widgetSourceId) return;
        if (!!value) {
            this.widgetHtmlService.getView(value)
                .then(view => this.htmlView = view)
                .catch(error => this.messageService.add(Message.Error(error.message, "Error loading chart")));
        } else this.htmlView = null;
    }

    get widgetSourceId() {
        return this._widgetSource?.id;
    }

    get isDraftWidget(): boolean {
        return !!this._widgetSource?.isDraft;
    }

    get valid(): boolean {
        return !!this.htmlCode;
    }


    // Refreshing methods.
    private async refreshWidget(): Promise<void> {
        this.htmlCode = this.htmlView?.innerHtml ?? "";
    }

    protected refreshHtmlPreview(): void {
        this.htmlPreview = {
            id: this.htmlView?.id,
            innerHtml: this.htmlCode,
            widgetSource: this._widgetSource,
            widgetSourceId: this._widgetSource?.id
        };
    }

    private getHtmlBuild(): IHtmlDTO {
        return {
            id: this.htmlView?.id,
            innerHtml: this.htmlCode ?? "",
            widgetSource: this._widgetSource,
            widgetSourceId: this._widgetSource?.id
        };
    }

    // Edition methods.
    async createDraft(): Promise<string> {
        const widgetSourceId: string = this.isDraftWidget
            ? this._widgetSource.releaseWidgetId
            : this.widgetSourceId;

        const editionFunc = (html: IHtmlDTO): Promise<IHtmlDTO> =>
            this.widgetHtmlService.createDraft(html, widgetSourceId);

        return this.editHtml(editionFunc);
    }

    async releaseDraft(): Promise<string> {
        if (!this.widgetSourceId) return;

        const editionFunc = (html: IHtmlDTO): Promise<IHtmlDTO> =>
            this.widgetHtmlService.update(html.id, html)
                .then(build => this.widgetHtmlService.releaseDraft(build.widgetSourceId)
                    .then(widgetSourceId => this.widgetHtmlService.getView(widgetSourceId)));

        return this.editHtml(editionFunc);
    }

    async save(): Promise<string> {
        const editionFunc = (html: IHtmlDTO): Promise<IHtmlDTO> =>
            !this.widgetSourceId
                ? this.widgetHtmlService.create(html)
                : this.widgetHtmlService.update(html.id, html);

        return this.editHtml(editionFunc);
    }

    private async editHtml(editionFunc: (html: IHtmlDTO) => Promise<IHtmlDTO>): Promise<string> {
        this.loading = true;
        const html: IHtmlDTO = this.getHtmlBuild();

        // Try to edit the html. Restore html if edition fails.
        const build: IHtmlDTO = await editionFunc(html).catch(error => {
            this.messageService.add(Message.Error(`There was an error saving entity:\n${error.error}`));
            return null;
        });

        // Update the build only if there was no error.
        if (build != null) this.htmlView = build as IHtmlViewDTO;

        this.loading = false;
        return build?.widgetSourceId ?? null;
    }
}