import {
    Component,
    ElementRef,
    EventEmitter,
    Input,
    OnDestroy,
    Output,
    ViewChild, ViewContainerRef
} from "@angular/core";
import {EmbeddedWidget, IHtmlViewDTO} from "@main/reporting.v3/widget.html/widget-html.models";
import {Message} from "@bbwt/classes";
import {WidgetHtmlService} from "@main/reporting.v3/widget.html/api/widget-html.service";
import {MessageService} from "primeng/api";
import {VariableRuleService} from "@main/reporting.v3/core/variables/variable-rule.service";
import {VariableHubService} from "@main/reporting.v3/core/variables/variable-hub.service";
import {IVariableReceiver} from "@main/reporting.v3/core/variables/variable-receiver";
import {IEmittedVariable, IVariableRule} from "@main/reporting.v3/core/variables/variable-models";
import {IWidgetComponent} from "@main/reporting.v3/core/widget-component";
import {PdfConfiguration} from "@main/reporting.v3/core/reporting-models";
import {PdfExportingService} from "@main/reporting.v3/api/pdf-exporting.service";
import {firstValueFrom} from "rxjs";
import {v4 as uuidv4} from "uuid";
import {WidgetComponent} from "@main/reporting.v3/widget/widget.component";
import {IHash} from "@bbwt/interfaces";
import {WidgetControlSetComponent} from "@main/reporting.v3/widget.control-set/components/widget-control-set.component";
import {IFilterItem} from "@features/filter";


@Component({
    selector: "widget-html",
    templateUrl: "./widget-html.component.html",
    styleUrls: ["./widget-html.component.scss"]
})
export class WidgetHtmlComponent implements IWidgetComponent, IVariableReceiver, OnDestroy {
    // Pdf view settings.
    renderPdfViewHidden: boolean;
    pdfViewContainer: ElementRef<HTMLElement>;

    // Variable handling settings.
    variableReceiverId: string = uuidv4();

    widgetTitle: string;

    public readonly widgetType: string = "html";

    private _htmlView: IHtmlViewDTO;
    private _widgetSourceId: string;
    private _widgetVisible: boolean;
    private _output: "web" | "PDF" = "web";
    private _htmlContainer: ElementRef<HTMLElement>;
    private lastEmittedVariables: IEmittedVariable[] = [];
    private embeddedWidgetsByCode: IHash<EmbeddedWidget[]> = {};

    // Shows or hides exporting button.
    @Input() exportable: boolean = true;

    // Flag to ignore widget visibility rules (for preview purposes).
    @Input() ignoreDisplayRule: boolean;

    // Emitter to notify that pdf view is ready for exporting.
    @Output() pdfExportingReady: EventEmitter<void> = new EventEmitter<void>();

    constructor(private widgetHtmlService: WidgetHtmlService,
                private variableHubService: VariableHubService,
                private variableRuleService: VariableRuleService,
                private pdfExportingService: PdfExportingService,
                private messageService: MessageService,
                private containerRef: ViewContainerRef) {
    }

    // Captures pdf view container and notify when it is ready.
    @ViewChild("pdfViewContainer")
    protected set htmlPdfView(value: ElementRef<HTMLElement>) {
        if (!!value) {
            setTimeout(() => {
                this.pdfViewContainer = value;
                this.pdfExportingReady.emit();
            }, 100);

        } else this.pdfViewContainer = null;
    }

    @ViewChild("htmlContainer")
    protected set htmlContainer(value: ElementRef<HTMLElement>) {
        this._htmlContainer = value;
        this.refreshHtmlContainer(value?.nativeElement);
    }

    get htmlContainer(): ElementRef<HTMLElement> {
        return this._htmlContainer;
    }

    // Determines which html code should be used for rendering: web view of pdf exporting view.
    @Input() set output(value: "web" | "PDF") {
        this._output = value ?? "web";
        this.getEmbeddedWidgets().forEach(widget => widget.component.instance.output = value);
    }

    get output(): "web" | "PDF" {
        return this._output;
    }

    // Determines if html is able to receive variables.
    @Input() set variableReceiver(value: boolean) {
        if (!!value || value == null) this.variableHubService.registerVariableReceiver(this);
        else this.variableHubService.unregisterVariableReceiver(this);
    }

    // Html view for updating purposes.
    @Input() set htmlView(value: IHtmlViewDTO) {
        if (!value || value === this._htmlView) return;
        this._htmlView = value;
        this.refreshWidget();
    }

    get htmlView() {
        return this._htmlView;
    }

    // Widget source id for updating purposes.
    @Input() set widgetSourceId(value: string) {
        if (!value || value === this._widgetSourceId) return;
        this.widgetHtmlService.getView(value)
            .then(view => this.htmlView = view)
            .catch(error => this.messageService.add(Message.Error(error.message, "Error loading chart")));
    }

    get widgetSourceId() {
        return this._widgetSourceId;
    }

    @Input() set widgetVisible(value: boolean) {
        this._widgetVisible = value;
    }

    get widgetVisible(): boolean {
        return this.ignoreDisplayRule || this._widgetVisible;
    }


    ngOnDestroy(): void {
        this.variableHubService.unregisterVariableReceiver(this);
    }

    // Variable management methods.
    receiveEmittedVariables(variables: IEmittedVariable[]): void {
        // Save current state of variables.
        const oldEmittedVariables: IEmittedVariable[] = this.lastEmittedVariables ?? [];

        //TODO: consider further improvement: avoid redundant refreshing widget view on ANY variable received.
        //To do that, on widget init we would need to build a list of widget-related variables from widget's parts,
        //dependent on variables (title, HTML content).

        // Take as last emitted variables only non-empty variables.
        this.lastEmittedVariables = variables.filter(variable => !variable.empty);

        if (!this.htmlView) return;

        // Show/hide widget variable.
        const displayRule: IVariableRule = this.htmlView.widgetSource?.displayRule;

        this.widgetVisible = !displayRule || this.variableRuleService.isMatch(displayRule,
            variables.find(variable => displayRule.variableName === variable.name));

        const isNewVariable = (variable: IEmittedVariable) => !oldEmittedVariables
            .some(oldVariable => this.variableRuleService.equalVariables(variable, oldVariable));
        
        // Refresh the widget if some related variable changed its value.
        if (this.lastEmittedVariables.some(isNewVariable)) {
            this.refreshWidget();
        }   
    }

    private refreshWidget(): void {
        this.refreshTitle();
        this.refreshHtmlContent();
    }

    private refreshTitle(): void {
        if (!this.htmlView?.widgetSource?.title) return;
        this.widgetTitle = this.variableRuleService.embedVariableValues(this.htmlView.widgetSource?.title, this.lastEmittedVariables);
    }

    // Refreshing methods.
    private refreshHtmlContent(): void {
        if (!this.htmlView) return;

        this.refreshHtmlContainer(this.htmlContainer?.nativeElement);

        if (this.widgetVisible != null) return;
        this.widgetVisible = this.ignoreDisplayRule || !this.htmlView.widgetSource?.displayRule;
    }

    private refreshHtmlContainer(container: HTMLElement, document?: Document): void {
        if (!container) return;

        document ??= this.getHtmlDocument();

        // Replace container children with document children.
        container.replaceChildren(...Array.from(document.body.children));
    }

    // Auxiliary methods.
    private getHtmlDocument(): Document {
        if (!this.htmlView) return;

        let html: string = this.htmlView.innerHtml ?? "";

        // Substitute embedded variable names by its current values.
        html = this.variableRuleService.embedVariableValues(html, this.lastEmittedVariables);

        // Remove script tags.
        const replacer = (_: string, group: string): string => `<p>${group}</p>`;
        html = html.replace(/<script.*?>(.*?)<\/script>/g, replacer);

        // Create new node tree.
        const parser: DOMParser = new DOMParser();
        const document = parser.parseFromString(html, "text/html");

        // Reset status of all cached widgets to "non-taken".
        this.prepareWidgetCache();

        // Render inner widgets from widget-codes embedded inside the html code.
        this.renderEmbeddedWidgets(document.body);

        // Clean cache from unused widgets.
        this.cleanWidgetCache();

        return document;
    }

    private renderEmbeddedWidgets(node: HTMLElement): void {
        if (!node) return;

        const type: number = node.nodeType;
        const children: ChildNode[] = Array.from(node.childNodes);

        for (let i: number = children.length - 1; i >= 0; i--) {
            // Recursively unwrap child nodes.
            this.renderEmbeddedWidgets(node.childNodes[i] as HTMLElement);
        }

        if (type === Node.TEXT_NODE || type === Node.ELEMENT_NODE) {
            const content: string = node.textContent;
            const parent: ParentNode = node.parentNode;
            const matches = content.match(/(?:^|.*?)(\[widget:[a-zA-Z0-9\-_ ]+])(.*?)(?=\[widget:[a-zA-Z0-9\-_ ]+]|$)/g);

            matches?.forEach(match => {
                const groups = match.match(/(?<prefix>.*)\[widget:(?<code>[a-zA-Z0-9\-_]+)](?<suffix>.*)/)?.groups;
                const code = groups?.["code"];

                if (!!code) {
                    // Get the first non-taken widget with this widget-code.
                    let embeddedWidget = this.embeddedWidgetsByCode[code]?.find(widget => !widget.taken);

                    // If there is no free widgets with this widget code, create a new one.
                    if (!embeddedWidget) {
                        this.embeddedWidgetsByCode[code] ??= [];

                        // Build the component corresponding to this widget code.
                        const widget = this.containerRef.createComponent<WidgetComponent>(WidgetComponent);
                        widget.instance.code = code;
                        embeddedWidget = {component: widget, taken: true};
                        this.embeddedWidgetsByCode[code].push(embeddedWidget);

                        // Otherwise, simply mark it as taken.
                    } else embeddedWidget.taken = true;

                    // Get the node of this widget component.
                    embeddedWidget.component.instance.output = this.output;
                    const widgetNode: Node = embeddedWidget.component.location.nativeElement;

                    // Substitute the text node by the corresponding widget.
                    parent.insertBefore(widgetNode, node);

                    if (!!groups["prefix"]) {
                        const prefixElement = node.cloneNode();
                        prefixElement.textContent = groups["prefix"];
                        parent.insertBefore(prefixElement, widgetNode);
                    }

                    if (!!groups["suffix"]) {
                        const suffixElement = node.cloneNode();
                        suffixElement.textContent = groups["suffix"];
                        parent.insertBefore(suffixElement, widgetNode.nextSibling);
                    }
                }
            });

            // Remove this node.
            if (!!matches?.length) parent.removeChild(node);
        }
    }

    private getEmbeddedWidgets(): EmbeddedWidget[] {
        return Object.keys(this.embeddedWidgetsByCode).flatMap(code => this.embeddedWidgetsByCode[code]);
    }

    private prepareWidgetCache(): void {
        this.getEmbeddedWidgets().forEach(widget => widget.taken = false);
    }

    private cleanWidgetCache(): void {
        // Destroy all unused widgets.
        this.getEmbeddedWidgets().filter(widget => !widget.taken).forEach(widget => widget.component.destroy());

        // Filter the cache from unused widgets.
        Object.keys(this.embeddedWidgetsByCode).forEach(code =>
            this.embeddedWidgetsByCode[code] = this.embeddedWidgetsByCode[code].filter(widget => widget.taken));
    }

    // Styling methods.
    private embeddedWidgetVisible(component: IWidgetComponent, output: "PDF" | "web" = this.output): boolean {
        if (!component) return false;

        // In the pdf view, the set of controls is only visible if at least one filter is filled.
        if (output === "PDF" && component.widgetType === "control-set") {
            const controlSet: WidgetControlSetComponent = component as WidgetControlSetComponent;
            const filters: { [key: string]: IFilterItem } = controlSet.filters;
            if (!filters) return false;
            return !!component.widgetVisible && Object.keys(filters).some(key => !!filters[key].value)
        }

        // Determines if a widget is visible.
        return !!component.widgetVisible;
    }

    // Pdf exporting methods.
    private async renderPdfViewForFunction<T>(func: () => Promise<T>): Promise<T> {
        if (this.pdfViewContainer) return func();

        // Render pdf view hidden, and then execute parameter function.
        this.renderPdfViewHidden = true;
        await firstValueFrom(this.pdfExportingReady);

        const configurations: IHash<PdfConfiguration[]> = {};

        await Promise.all(Object.keys(this.embeddedWidgetsByCode).map(async code => {
            // Get all widget components with this widget code.
            const widgets: IWidgetComponent[] = this.embeddedWidgetsByCode[code]
                .map(widget => widget.component?.instance?.widgetComponent);

            // Get corresponding pdf configurations.
            configurations[code] = await Promise.all(widgets
                .map(component => this.embeddedWidgetVisible(component, "PDF") ? component.getPdfConfiguration() : null));
        }));

        const index: IHash<number> = {};
        Object.keys(configurations).forEach(key => index[key] = 0);
        const pattern: RegExp = /\[widget:(?<code>[a-zA-Z0-9\-_]+)]/g;

        const htmlCode = this.htmlView.innerHtml.replace(pattern, (_: string, code: string) => {
            if (!code || index[code] == null || index[code] >= configurations[code].length) return "";

            const configuration: PdfConfiguration = configurations[code][index[code]++];
            if (!configuration) return "";

            const html: string = configuration.htmlContent;
            const css: string = configuration.cssRules;

            return `${html}\n<style>${css}</style>`;
        });

        const parser: DOMParser = new DOMParser();
        const document: Document = parser.parseFromString(htmlCode, "text/html");
        const container: HTMLElement = this.pdfViewContainer.nativeElement.querySelector(".widget-html-content");

        this.refreshHtmlContainer(container, document);

        return func().finally(() => this.renderPdfViewHidden = false);
    }

    private getHtmlContent(): string {
        return this.pdfViewContainer?.nativeElement?.outerHTML;
    }

    private getWidth(): string {
        if (!this.pdfViewContainer?.nativeElement) return null;

        const zoom: number = this.pdfViewContainer.nativeElement.style["zoom"];
        const scale: number = window.devicePixelRatio;

        this.pdfViewContainer.nativeElement.style["zoom"] = scale > 0 ? 1 / scale : 1;
        const width: number = this.pdfViewContainer.nativeElement.offsetWidth;
        this.pdfViewContainer.nativeElement.style["zoom"] = zoom;

        return width.toString();
    }

    private getCssRules(): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("widget-html-pdf-view-container")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        return styleDeclaration
            .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
            .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "");
    }

    private getFooterTemplate(): string {
        return `<div class='widget-html-pdf-view-page-footer'>
                    Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                </div>`;
    }

    private getFooterCssRules(): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("widget-html-pdf-view-page-footer")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        return styleDeclaration
            .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
            .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "");
    }

    async getPdfConfiguration(): Promise<PdfConfiguration> {
        return !!this.pdfViewContainer
            ? {
                htmlContent: this.getHtmlContent(),
                cssRules: this.getCssRules(),
                width: this.getWidth(),
                footerTemplate: this.getFooterTemplate(),
                footerCssRules: this.getFooterCssRules(),
                margin: "10 20 40 20"
            } : await this.renderPdfViewForFunction<PdfConfiguration>(this.getPdfConfiguration.bind(this));
    }

    async generatePdf(): Promise<void> {
        const configuration: PdfConfiguration = await this.getPdfConfiguration();
        this.pdfExportingService.generateFromHtml(configuration)
            .then(blob => this.pdfExportingService.openPdf(blob));
    }
}