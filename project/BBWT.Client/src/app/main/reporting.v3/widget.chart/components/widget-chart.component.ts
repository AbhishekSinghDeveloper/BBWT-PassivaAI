import {
    Component,
    ElementRef,
    EventEmitter,
    HostListener,
    Input,
    OnDestroy,
    Output,
    QueryList,
    ViewChild,
    ViewChildren
} from "@angular/core";
import {WidgetChartService} from "../api/widget-chart.service";
import {
    ChartSettings,
    ChartTypeEnum,
    ColumnPurpose,
    groupBy,
    IChartBuildColumnDTO,
    IChartViewDTO,
    sortBy,
    toStandardString
} from "@main/reporting.v3/widget.chart/widget-chart.models";
import {Chart, ChartData, ChartDataset, ChartType, Plugin, TooltipModel} from "chart.js";
import {MessageService} from "primeng/api";
import {Message} from "@bbwt/classes";
import {IVariableReceiver} from "../../core/variables/variable-receiver";
import {VariableHubService} from "../../core/variables/variable-hub.service";
import {IEmittedVariable, IQueryVariables, IVariableRule} from "../../core/variables/variable-models";
import {VariableRuleService} from "../../core/variables/variable-rule.service";
import {AnyObject} from "chart.js/dist/types/basic";
import {IWidgetComponent} from "@main/reporting.v3/core/widget-component";
import {PdfConfiguration} from "@main/reporting.v3/core/reporting-models";
import {PdfExportingService} from "@main/reporting.v3/api/pdf-exporting.service";
import {UIChart} from "primeng/chart";
import {firstValueFrom} from "rxjs";
import {v4 as uuidv4} from "uuid";


@Component({
    selector: "widget-chart",
    templateUrl: "./widget-chart.component.html",
    styleUrls: ["./widget-chart.component.scss"]
})
export class WidgetChartComponent implements IWidgetComponent, IVariableReceiver, OnDestroy {
    // Chart settings.
    chartData: ChartData;
    dataGroups: any[][] = [];
    chartDatasets: ChartDataset[];
    chartSettings: ChartSettings;

    // Chart columns configurations.
    axisXQueryAlias: string;
    axisYQueryAlias: string;
    seriesQueryAlias: string;
    bubbleSizeQueryAlias: string;
    columnsOnTooltip: IChartBuildColumnDTO[] = [];

    // Dataset dialog settings.
    selectedDatasets: ChartDataset[] = [];
    datasetManagerVisible: boolean = false;

    // Pdf view settings.
    renderPdfViewHidden: boolean;
    pdfViewContainer: ElementRef<HTMLElement>;

    // Variable handling settings.
    variableReceiverId: string = uuidv4();

    widgetTitle: string;

    public readonly widgetType: string = "chart";

    private _widgetSourceId: string;
    private _widgetVisible: boolean;
    private lastEmittedVariables: IEmittedVariable[];
    private lastEmittedQueryVariables: IEmittedVariable[];

    @ViewChildren(UIChart) private charts: QueryList<UIChart>;

    // Shows or hides exporting button.
    @Input() exportable: boolean = true;

    // Determines which html code should be used for rendering: web view of pdf exporting view.
    @Input() output: "web" | "PDF" = "web";

    // Flag to ignore widget visibility rules (for preview purposes).
    @Input() ignoreDisplayRule: boolean;

    // Emitter to notify that pdf view is ready for exporting.
    @Output() pdfExportingReady: EventEmitter<void> = new EventEmitter<void>();

    constructor(private widgetChartService: WidgetChartService,
                private variableHubService: VariableHubService,
                private variableRuleService: VariableRuleService,
                private pdfExportingService: PdfExportingService,
                private messageService: MessageService) {
    }

    // Captures pdf view container and notify when it is ready.
    @ViewChild("pdfViewContainer")
    protected set chartPdfView(value: ElementRef<HTMLElement>) {
        if (!!value) {
            setTimeout(() => {
                this.pdfViewContainer = value;
                this.pdfExportingReady.emit();
            }, 100);

        } else this.pdfViewContainer = null;
    }

    // Determines if grid is able to receive variables.
    @Input() set variableReceiver(value: boolean) {
        if (!!value || value == null) this.variableHubService.registerVariableReceiver(this);
        else this.variableHubService.unregisterVariableReceiver(this);
    }

    // Base data of the chart. Refresh chart data every time this variable changes.
    @Input() set dataRows(value: any[]) {
        if (!value) return;
        this._dataRows = value;
        this.refreshChartData();
    }

    get dataRows(): any[] {
        return this._dataRows;
    }

    private _dataRows: any[] = [];

    // View of the chart. Refresh chart settings every time this variable changes.
    @Input() set chartView(value: IChartViewDTO) {
        if (!value) return;
        this._chartView = value;

        this.refreshTitle();
        this.refreshChartSettings();
    };

    get chartView(): IChartViewDTO {
        return this._chartView;
    }

    private _chartView: IChartViewDTO;

    // Widget source of the chart. Refresh all widget every time this variable changes.
    @Input() set widgetSourceId(value: string) {
        if (this.widgetSourceId === value) return;
        this._widgetSourceId = value;
        this.refreshWidget().then();
    }

    get widgetSourceId(): string {
        return this._widgetSourceId;
    }

    @Input() set widgetVisible(value: boolean) {
        this._widgetVisible = value;
    }

    get widgetVisible(): boolean {
        return this.ignoreDisplayRule || this._widgetVisible;
    }

    get hasFixedSize(): boolean {
        if (!this.charts?.last) return false;
        return !this.charts.last.width.endsWith("vw") && !this.charts.last.width.endsWith("%");
    }


    @HostListener("window:resize")
    onResize(): void {
        if (this.hasFixedSize && this.charts.last.options?.responsive) {
            this.charts.last.refresh();
        }
    }

    ngOnDestroy(): void {
        this.variableHubService.unregisterVariableReceiver(this);
    }

    // Variable management methods.
    receiveEmittedVariables(variables: IEmittedVariable[]): void {
        // Save current state of variables.
        const oldEmittedVariables: IEmittedVariable[] = this.lastEmittedVariables ?? [];
        // If there is no widget yet, take all variables as last emitted variables.
        // Otherwise, take as last emitted variables only the variables related to this widget.
        this.lastEmittedVariables = variables.filter(variable => !variable.empty);

        const oldEmittedQueryVariables: IEmittedVariable[] = this.lastEmittedQueryVariables ?? [];
        this.lastEmittedQueryVariables = !this.chartView ? variables : variables.filter(variable =>
            this.chartView.queryVariables?.some(queryVariable => queryVariable === variable.name));


        if (!this.chartView) return;

        // Show/hide widget if required.
        const displayRule: IVariableRule = this.chartView.widgetSource?.displayRule;

        this.widgetVisible = !displayRule || this.variableRuleService.isMatch(displayRule,
            variables.find(variable => displayRule.variableName === variable.name));

        const isNewVariable = (variable: IEmittedVariable) => !oldEmittedVariables
            .some(oldVariable => this.variableRuleService.equalVariables(variable, oldVariable));

        const isNewQueryVariable = (variable: IEmittedVariable) => !oldEmittedQueryVariables
            .some(oldVariable => this.variableRuleService.equalVariables(variable, oldVariable));

        if (this.lastEmittedVariables.some(isNewVariable)) this.refreshTitle();

        // Refresh the widget if some related variable changed its value.
        if (this.lastEmittedQueryVariables.some(isNewQueryVariable)) this.reloadChart().then();

    }

    // Refreshing methods.
    private async refreshWidget(): Promise<void> {
        if (!this.widgetSourceId) return;
        this.chartView = await this.widgetChartService.getView(this.widgetSourceId);
        this.dataRows = await this.widgetChartService.getQueryDataRows(this.chartView.querySourceId);
    }

    private refreshChartData(): void {
        if (!this.dataRows) return;

        this.dataGroups = groupBy(this.dataRows, this.seriesQueryAlias);

        // Update data, declaring a dataset foreach group.
        this.chartDatasets = sortBy(this.dataGroups.map((group, i) => {
            // Set dataset label as the value of the series column of the first entry of the group.
            const label: string = group[0]?.[this.seriesQueryAlias] ?? "No data";

            // Set data as the values of the y-axis column of each element of the group.
            const data: number[] = sortBy(group, this.axisXQueryAlias)
                .map(data => data[this.axisYQueryAlias]);

            // Set radius only if required (chart is bubble chart and bubble size alias is set).
            let radius: number[];
            if (!!this.bubbleSizeQueryAlias && !!group.length) {
                // Set radius as the values of the bubble size column of each element of the group.
                radius = group.map(data => data[this.bubbleSizeQueryAlias]);
                // Calculate a constant to normalize bubble sizes due respect the amount of bubbles.
                const max = Math.max(...radius);
                const constant = 5000 / radius.length;
                // Use the constant to normalize the radius.
                radius = radius.map(data => (data * constant) / max);
            }

            // Find old version of this dataset.
            const oldDataset: ChartDataset = this.chartDatasets?.find(dataset => dataset.label === label);
            // Set hidden status as the status of the old dataset, if exists.
            const hidden: boolean = !!oldDataset ? oldDataset.hidden : i > 10;

            return <ChartDataset>{
                label: label,
                index: i,
                data: data,
                radius: radius,
                hidden: hidden
            }
        }), "label");

        // Set datasets as the visible datasets of the chart (to avoid loading issues).
        const datasets: ChartDataset[] = this.chartDatasets.filter(dataset => !dataset.hidden);

        // Set dataset labels as the value of the x-axis column of each element of first group.
        const labels: string[] = toStandardString(sortBy(this.dataGroups[0], this.axisXQueryAlias)
            .map(data => data[this.axisXQueryAlias]));

        this.chartData = {labels: labels, datasets: datasets};
    }

    private refreshTitle(): void {
        if (!this.chartView?.widgetSource?.title) return;
        this.widgetTitle = this.variableRuleService.embedVariableValues(this.chartView.widgetSource?.title, this.lastEmittedVariables);
    }

    private refreshChartSettings(): void {
        if (!this.chartView) return;

        const oldSeriesQueryAlias: string = this.seriesQueryAlias;

        this.seriesQueryAlias = this.chartView.columns
            .find(column => column.columnPurpose === ColumnPurpose.Series)?.queryAlias;
        this.axisXQueryAlias = this.chartView.columns
            .find(column => column.columnPurpose === ColumnPurpose.AxisX)?.queryAlias;
        this.axisYQueryAlias = this.chartView.columns
            .find(column => column.columnPurpose === ColumnPurpose.AxisY)?.queryAlias;
        this.bubbleSizeQueryAlias = this.chartView.columns
            .find(column => column.columnPurpose === ColumnPurpose.BubbleSize)?.queryAlias;
        this.columnsOnTooltip = this.chartView.columns
            .filter(column => column.columnPurpose === ColumnPurpose.Tooltip);

        if (this.seriesQueryAlias !== oldSeriesQueryAlias) this.chartDatasets = null;

        let settings: ChartSettings = null;

        try {
            if (!!this.chartView.chartSettingsJson) settings = JSON.parse(this.chartView.chartSettingsJson);
        } catch (error) {
            this.messageService.add(Message.Warning(error.message, "Invalid chart settings object"));
        }

        if (!settings) settings = {type: ChartTypeEnum.Line, width: "500px", height: "500px"};
        if (!settings.options) settings.options = {aspectRatio: 0.8, maintainAspectRatio: false};
        if (!settings.options.plugins) settings.options.plugins = {};
        if (!settings.options.plugins.tooltip) settings.options.plugins.tooltip = {};
        if (!settings.options.animation) settings.options.animation = {duration: 0};

        settings.options.plugins.tooltip = {
            ...settings.options.plugins.tooltip,
            enabled: !!this.columnsOnTooltip.length,
            external: this.getTooltipFunction()
        };

        settings.plugins = [this.getBackgroundPlugin()];

        this.chartSettings = settings;

        if (this.widgetVisible != null) return;
        this.widgetVisible = this.ignoreDisplayRule || !this.chartView.widgetSource?.displayRule;
    }

    private async reloadChart(): Promise<void> {
        if (!this.chartView) return;

        // Fetch chart query data for the new set of variables. Refresh chart data.
        const queryVariables: IQueryVariables = {variables: this.lastEmittedVariables ?? []};
        this.dataRows = await this.widgetChartService.getQueryDataRows(this.chartView.querySourceId, queryVariables);
    }

    // Dataset manager methods.
    protected showDatasetManager(): void {
        if (!this.chartData) return;
        this.selectedDatasets = [...this.chartData.datasets];
        this.datasetManagerVisible = true;
    }

    protected changeDatasetsVisibility(): void {
        if (!this.chartData) return;
        this.chartDatasets.forEach(dataset => dataset.hidden = true);
        this.selectedDatasets.forEach(dataset => dataset.hidden = false);
        this.chartData.datasets = sortBy(this.selectedDatasets, "label");
        this.hideDatasetManager();
        this.charts.last.refresh();
    }

    protected hideDatasetManager(): void {
        this.datasetManagerVisible = false;
        this.selectedDatasets = [];
    }

    // Auxiliary methods.
    private getBackgroundPlugin(): Plugin {
        return {
            id: "canvasBackground",
            beforeDraw(chart: Chart<ChartType>, _: { cancelable: true }, options: AnyObject): void {
                const {ctx} = chart;
                ctx.save();
                ctx.globalCompositeOperation = "destination-over";
                ctx.fillStyle = options.color || "#f4f4f4";
                ctx.fillRect(0, 0, chart.width, chart.height);
                ctx.restore();
            }
        }
    }

    private getTooltipFunction(): (args: { chart: Chart; tooltip: TooltipModel<ChartType> }) => void {
        return (context) => {
            const {tooltip} = context;

            if (!tooltip.$animations || !this.columnsOnTooltip?.length) return;

            const dataIndex: number = tooltip.dataPoints[0].dataIndex;
            const datasetIndex: number = tooltip.dataPoints[0].dataset["index"];
            const data = this.dataGroups[datasetIndex][dataIndex];

            const columns: string[] = this.columnsOnTooltip.map(column =>
                `${column.chartAlias}: ${data[column.queryAlias]}`.slice(0, 50));

            tooltip.height = 30 + 17.5 * columns.length;
            tooltip.width = 8 * Math.max(...columns.map(data => data.length));
            tooltip.$animations.height.update(tooltip.$animations.height, tooltip.height, null);
            tooltip.$animations.width.update(tooltip.$animations.width, tooltip.width, null);

            tooltip.title = [data[this.seriesQueryAlias] ?? ""];

            tooltip.body = columns.map(data => <any>{after: [], before: [], lines: [data]});

            tooltip.labelTextColors = columns.map(_ => tooltip.labelTextColors[0]);
            tooltip.labelColors = columns.map(_ => tooltip.labelColors[0]);
            tooltip.labelPointStyles = columns.map(_ => tooltip.labelPointStyles[0]);
        }
    }

    // Pdf exporting methods.
    private async renderPdfViewForFunction<T>(func: () => Promise<T>): Promise<T> {
        if (this.pdfViewContainer) return func();

        // Render pdf view hidden, and then execute parameter function.
        this.renderPdfViewHidden = true;
        await firstValueFrom(this.pdfExportingReady);

        return func().finally(() => this.renderPdfViewHidden = false);
    }

    private getHtmlContent(): string {
        if (!this.pdfViewContainer?.nativeElement || !this.charts?.last) return null;

        const chart: Chart = this.charts.last.chart;
        const scale: number = window.devicePixelRatio;
        const htmlContent: string = this.pdfViewContainer.nativeElement.outerHTML;

        if (this.hasFixedSize) {
            // Apply reverse pixel ratio only if it's greater than 1 (i.e, device pixel ratio is lower than 1).
            chart.options.devicePixelRatio = 0 < scale && scale < 1 ? 1 / scale : 1;
            chart.resize();
            chart.update();
        }

        const parser: DOMParser = new DOMParser();
        const doc: Document = parser.parseFromString(htmlContent, "text/html");
        const container: Element = doc.body.querySelector("p-chart");
        const canvas: HTMLCanvasElement = container.querySelector("canvas");
        const image: HTMLImageElement = doc.createElement("img");

        image.src = this.charts.last.getBase64Image();
        container.parentNode?.replaceChild(image, container);

        // Fix image size.
        if (this.hasFixedSize) {
            image.style.width = this.charts.last.width;
            image.style.height = this.charts.last.height;

            // Restore chart to old status.
            chart.options.devicePixelRatio = null;
            chart.resize();
            chart.update();
        } else {
            image.width = canvas.width;
            image.height = canvas.height;
        }

        return doc.body?.innerHTML;
    }

    private getWidth(): string {
        if (!this.pdfViewContainer?.nativeElement) return null;

        if (!this.hasFixedSize) {
            const canvas: HTMLCanvasElement = this.pdfViewContainer.nativeElement.querySelector("canvas");
            return (canvas.width * 2).toString();
        }

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
                if (!rule?.style || !rule.cssText?.includes("widget-chart-pdf-view-container")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        return styleDeclaration
            .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
            .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "");
    }

    private getFooterTemplate(): string {
        return `<div class='widget-chart-pdf-view-page-footer'>
                    Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                </div>`;
    }

    private getFooterCssRules(): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("widget-chart-pdf-view-page-footer")) continue;
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
                margin: "10 20 40 20",
            } : await this.renderPdfViewForFunction<PdfConfiguration>(this.getPdfConfiguration.bind(this));
    }

    async generatePdf(): Promise<void> {
        const configuration: PdfConfiguration = await this.getPdfConfiguration();
        this.pdfExportingService.generateFromHtml(configuration)
            .then(blob => this.pdfExportingService.openPdf(blob));
    }
}