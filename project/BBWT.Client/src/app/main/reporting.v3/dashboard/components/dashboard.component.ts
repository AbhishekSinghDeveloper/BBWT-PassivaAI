import {Component, ElementRef, Input} from "@angular/core";
import {DashboardService} from "../api/dashboard.service";
import {getLayoutTypeName, IDashboardView, IDashboardWidgetView,} from "@main/reporting.v3/dashboard/dashboard-models";
import {IPdfExportable, IWidgetComponent} from "@main/reporting.v3/core/widget-component";
import {PdfConfiguration} from "@main/reporting.v3/core/reporting-models";
import {PdfExportingService} from "@main/reporting.v3/api/pdf-exporting.service";
import {WidgetControlSetComponent} from "@main/reporting.v3/widget.control-set/components/widget-control-set.component";
import {IFilterItem} from "@features/filter";


@Component({
    selector: "reporting-dashboard",
    templateUrl: "./dashboard.component.html",
    styleUrls: ["./dashboard.component.scss"]
})
export class ReportingDashboardComponent implements IPdfExportable {
    // Dashboard settings.
    widgetComponents: IWidgetComponent[][] = [];
    dashboardGrid: IDashboardWidgetView[][] = [];

    protected readonly getLayoutTypeName = getLayoutTypeName;

    private _dashboardId: string;
    private _dashboardCode: string;
    private _dashboardView: IDashboardView;

    // Shows or hides exporting button.
    @Input() exportable: boolean = true;

    // Determines which html code should be used for rendering: web view of pdf exporting view.
    @Input() output: "web" | "PDF" = "web";

    constructor(private dashboardService: DashboardService,
                private pdfExportingService: PdfExportingService,
                private el: ElementRef) {
    }

    @Input() set dashboardView(value: IDashboardView) {
        if (!value) return;
        this._dashboardView = value;
        this.refreshDashboardGrid();
    };

    get dashboardView(): IDashboardView {
        return this._dashboardView;
    }

    @Input() set dashboardId(value: string) {
        if (!value) return;
        this._dashboardId = value;
        this.refreshDashboardById().then();
    }

    get dashboardId(): string {
        return this._dashboardId;
    }

    @Input() set code(value: string) {
        if (!value) return;
        this._dashboardCode = value;
        this.refreshDashboardByCode().then();
    }

    get code(): string {
        return this._dashboardCode;
    }

    // Refreshing methods.
    private async refreshDashboardById(): Promise<void> {
        if (!this._dashboardId) return;
        this.dashboardView = await this.dashboardService.getView(this._dashboardId);
    }

    private async refreshDashboardByCode(): Promise<void> {
        if (!this._dashboardCode) return;
        this.dashboardView = await this.dashboardService.getViewByCode(this._dashboardCode);
    }

    private refreshDashboardGrid(): void {
        // Get upperbound of the amount of rows.
        const rows: number = !!this.dashboardView?.widgets?.length ?
            Math.max(...this.dashboardView.widgets.map(widget => widget.rowIndex)) + 1 : 0;

        if (!rows) return;

        // Get a dashboard grid row.
        const getDashboardGridRow = (index: number): IDashboardWidgetView[] => this.dashboardView.widgets
            .filter(widget => widget?.rowIndex === index)
            .sort((first, second) => first.columnIndex - second.columnIndex)

        // Declare a dashboard with that amount of rows.
        this.dashboardGrid = Array(rows + 1).fill(null)
            // Get dashboard widgets corresponding to this row.
            .map((_, i) => getDashboardGridRow(i))
            // Remove empty rows from the dashboard grid.
            .filter(row => row.length > 0);

        // Declare widget components grid with same dimensions as dashboard.
        this.widgetComponents = Array(this.dashboardGrid.length).fill(null)
            .map((_, i) => Array(this.dashboardGrid[i].length))
    }

    protected refreshWidget(component: IWidgetComponent, row: number, column: number): void {
        if (this.output !== component.output) return;
        this.widgetComponents[row][column] = component;
    }

    // Styling methods.
    private widgetVisible(component: IWidgetComponent, output: "PDF" | "web" = this.output): boolean {
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

    protected widgetStyle(widget: IDashboardWidgetView): string {
        if (!this.widgetComponents || !this.dashboardView) return "";

        const row: IWidgetComponent[] = this.widgetComponents[widget.rowIndex];
        if (!row?.length) return "";

        const visibleComponents: IWidgetComponent[] = row
            .filter(component => this.widgetVisible(component));
        if (!visibleComponents.length) return "";

        const padding: number = this.dashboardView.widgetsPadding;
        const margin: number = this.dashboardView.widgetsMargin;
        const width: number = 100 / visibleComponents.length;

        return `padding: ${padding}px; width: calc(${width}% - ${margin}px);`;
    }

    protected widgetClass(widget: IDashboardWidgetView): string {
        const styleClass: string = "dashboard-widget";
        if (!this.widgetComponents?.length) return styleClass;

        const row: IWidgetComponent[] = this.widgetComponents[widget.rowIndex];
        if (!row?.length) return styleClass;

        const component: IWidgetComponent = row[widget.columnIndex];
        return this.widgetVisible(component) ? styleClass : styleClass + " hidden";
    }

    protected separatorStyle(_: number, column?: number): string {
        const margin: number = this.dashboardView.widgetsMargin ?? 0;
        const padding: number = this.dashboardView.widgetsPadding ?? 0;

        return column == null
            ? `padding: ${margin / 2}px ${padding * 2}px;`
            : `padding: ${padding * 2}px ${margin / 2}px;`;
    }

    protected separatorClass(row: number, column?: number): string {
        let styleClass: string = column == null
            ? "dashboard-widget-row-separator"
            : "dashboard-widget-separator";
        if (!this.widgetComponents?.length) return styleClass;

        if (column == null && row > 0) {
            const visibleComponents: boolean = this.widgetComponents[row - 1]
                .some(component => this.widgetVisible(component));
            if (!visibleComponents) styleClass += " hidden";

        } else if (column > 0) {
            const visibleComponent: boolean = this.widgetVisible(this.widgetComponents[row][column - 1]);
            if (!visibleComponent) styleClass += " hidden";
        }

        return styleClass;
    }

    // Pdf exporting methods.
    private getHtmlContent(configurations: PdfConfiguration[][]): string {
        const htmlContent: string = this.el.nativeElement?.outerHTML;
        if (!htmlContent?.length) return null;

        const parser: DOMParser = new DOMParser();
        const doc: Document = parser.parseFromString(htmlContent, "text/html");

        doc.querySelector(".dashboard-container")
            ?.setAttribute("class", "dashboard-pdf-view-container");

        const rows: NodeListOf<Element> = doc.querySelectorAll(".dashboard-widget-row");
        const rowSeparators: NodeListOf<Element> = doc.querySelectorAll(".dashboard-widget-row-separator");

        for (let i = 0; i < this.widgetComponents.length; i++) {
            const widgets: NodeListOf<Element> = rows[i].querySelectorAll(".dashboard-widget");
            const widgetSeparators: NodeListOf<Element> = rows[i].querySelectorAll(".dashboard-widget-separator");

            if (!!configurations[i].some(configuration => !!configuration)) {
                // If at least one widget in this row is visible.
                for (let j = 0; j < this.widgetComponents[i].length; j++) {
                    const configuration: PdfConfiguration = configurations[i][j];

                    if (!!configuration) {
                        // If widget is visible, substitute its current html with its pdf exporting html.
                        widgets[j].innerHTML = configuration.htmlContent ?? "";

                    } else {
                        // Otherwise, remove the widget from view.
                        rows[i].removeChild(widgets[j]);
                        rows[i].removeChild(widgetSeparators[j + 1]);
                    }
                }
            } else {
                // Otherwise, remove this row from view.
                rows[i].parentElement?.removeChild(rows[i]);
                rowSeparators[i + 1].parentElement?.removeChild(rowSeparators[i + 1]);
            }
        }

        return doc.body?.innerHTML;
    }

    private getWidth(configurations: PdfConfiguration[][]): string {
        if (!this.dashboardView) return null;

        const minWidth: number = 1470;
        const margin: number = this.dashboardView.widgetsMargin;

        const sumWidgetsWidth = (row: PdfConfiguration[]) => row
            .reduce((width, configuration) => width + parseFloat(configuration.width), 0);

        const widths: number[] = configurations
            .map(row => row.filter(configuration => !!configuration))
            .map(row => sumWidgetsWidth(row) + margin * row.length);

        return Math.max(...widths, minWidth).toString();
    }

    private getCssRules(configurations: PdfConfiguration[][]): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("dashboard-pdf-view-container")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        const componentStyles: Map<string, string> = new Map<string, string>();

        for (let i = 0; i < this.widgetComponents.length; i++) {
            for (let j = 0; j < this.widgetComponents[i].length; j++) {
                const component: IWidgetComponent = this.widgetComponents[i][j];
                const configuration: PdfConfiguration = configurations[i][j];

                if (!configuration || componentStyles.has(component.widgetType)) continue;
                componentStyles.set(component.widgetType, configuration.cssRules);
            }
        }

        return styleDeclaration
                .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
                .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "")
            + Array.from(componentStyles.values()).join("\n");
    }

    private getFooterTemplate(): string {
        return `<div class='dashboard-pdf-view-page-footer'>
                    Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                </div>`;
    }

    private getFooterCssRules(): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("dashboard-pdf-view-page-footer")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        return styleDeclaration
            .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
            .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "");
    }

    async getPdfConfiguration(): Promise<PdfConfiguration> {
        const configurations: PdfConfiguration[][] = await
            Promise.all(this.widgetComponents.map(components =>
                Promise.all(components.map(component =>
                    this.widgetVisible(component, "PDF") ? component.getPdfConfiguration() : null))));

        return {
            htmlContent: this.getHtmlContent(configurations),
            cssRules: this.getCssRules(configurations),
            width: this.getWidth(configurations),
            footerTemplate: this.getFooterTemplate(),
            footerCssRules: this.getFooterCssRules(),
            margin: "10 20 40 20"
        };
    }

    async generatePdf(): Promise<void> {
        const configuration: PdfConfiguration = await this.getPdfConfiguration();
        this.pdfExportingService.generateFromHtml(configuration)
            .then(blob => this.pdfExportingService.openPdf(blob));
    }
}