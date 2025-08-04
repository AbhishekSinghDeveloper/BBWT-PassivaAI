import {Component, EventEmitter, Input, Output, ViewChild} from "@angular/core";
import {IWidgetComponent} from "@main/reporting.v3/core/widget-component";
import {WidgetGridComponent} from "@main/reporting.v3/widget.grid/components/widget-grid.component";
import {WidgetHtmlComponent} from "@main/reporting.v3/widget.html/components/widget-html.component";
import {WidgetChartComponent} from "@main/reporting.v3/widget.chart/components/widget-chart.component";
import {WidgetControlSetComponent} from "@main/reporting.v3/widget.control-set/components/widget-control-set.component";
import {IWidgetSourcePreload, WidgetSourceCode} from "../core/reporting-models";
import {WidgetSourceService} from "../api/widget-source.service";

@Component({
    selector: "reporting-widget",
    templateUrl: "./widget.component.html",
    styleUrls: ["./widget.component.scss"]
})
export class WidgetComponent {
    // Inner widget component.
    widgetComponent: IWidgetComponent;

    // Shows or hides exporting button.
    @Input() exportable: boolean = true;

    // Determines which html code should be used for rendering: web view of pdf exporting view.
    @Input() output: "web" | "PDF" = "web";

    @Input() widgetType: WidgetSourceCode;
    @Input() widgetSourceId: string;
    @Input() variableEmitter: boolean;
    @Input() variableReceiver: boolean;
    @Input() ignoreDisplayRule: boolean;

    // Emitter to notify when inner widget component is ready or changes.
    @Output() widgetComponentChange: EventEmitter<IWidgetComponent> = new EventEmitter<IWidgetComponent>();

    constructor(private widgetSourceService: WidgetSourceService) {
    }

    // Widget code is supposed to be set from outside the reporting feature.
    @Input() set code(value: string) {
        this.widgetSourceService.getWidgetSourcePreload(value)
            .then((widgetSourcePreload: IWidgetSourcePreload): void => {
                this.widgetType = widgetSourcePreload.widgetType;
                this.widgetSourceId = widgetSourcePreload.id;
            });
    }

    @ViewChild(WidgetControlSetComponent) set widgetControlSetComponent(controlSet: WidgetControlSetComponent) {
        if (!controlSet) return;
        this.widgetComponent = controlSet;
        this.widgetComponentChange.emit(controlSet);
    }

    @ViewChild(WidgetChartComponent) set widgetChartComponent(chart: WidgetChartComponent) {
        if (!chart) return;
        this.widgetComponent = chart;
        this.widgetComponentChange.emit(chart);
    }

    @ViewChild(WidgetGridComponent) set widgetGridComponent(grid: WidgetGridComponent) {
        if (!grid) return;
        this.widgetComponent = grid;
        this.widgetComponentChange.emit(grid);
    }

    @ViewChild(WidgetHtmlComponent) set widgetHtmlComponent(html: WidgetHtmlComponent) {
        if (!html) return;
        this.widgetComponent = html;
        this.widgetComponentChange.emit(html);
    }

    get widgetVisible(): boolean {
        return this.widgetComponent?.widgetVisible;
    }

    generatePdf(): Promise<void> {
        return this.widgetComponent?.generatePdf();
    }
}