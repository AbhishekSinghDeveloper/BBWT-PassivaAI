import {NgModule} from "@angular/core";
import {WidgetComponent} from "@main/reporting.v3/widget/widget.component";
import {WidgetControlSetModule} from "@main/reporting.v3/widget.control-set/widget-control-set.module";
import {WidgetChartModule} from "@main/reporting.v3/widget.chart/widget-chart.module";
import {WidgetGridModule} from "@main/reporting.v3/widget.grid/widget-grid.module";
import {WidgetHtmlModule} from "@main/reporting.v3/widget.html/widget-html.module";
import {CommonModule} from "@angular/common";
import {PrimeNgModule} from "@primeng";
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";
import {WidgetSourceService} from "../api/widget-source.service";
import {WidgetBuilderComponent} from "@main/reporting.v3/widget/widget-builder.component";
import {ReportingCoreModule} from "@main/reporting.v3/reporting-core.module";


@NgModule({
    declarations: [
        WidgetComponent,
        WidgetBuilderComponent
    ],
    imports: [
        CommonModule,
        PrimeNgModule,
        BbwtSharedModule,

        WidgetControlSetModule,
        WidgetChartModule,
        WidgetGridModule,
        WidgetHtmlModule,
        ReportingCoreModule
    ],
    exports: [
        WidgetComponent,
        WidgetBuilderComponent
    ],
    providers: [WidgetSourceService],
})
export class WidgetModule {
}