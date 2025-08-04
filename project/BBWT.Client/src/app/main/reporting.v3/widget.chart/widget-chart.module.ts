import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {GridModule} from "@features/grid";
import {WidgetChartComponent} from "@main/reporting.v3/widget.chart/components/widget-chart.component";
import {WidgetChartBuilderComponent} from "@main/reporting.v3/widget.chart/components/widget-chart-builder.component";
import {PrimeNgModule} from "@primeng";
import {ChartModule} from "primeng/chart";
import {ColorPickerModule} from "primeng/colorpicker";
import {PrismService} from "@features/bb-formio/prism.service";
import {JsonEditorComponent} from "@main/reporting.v3/widget.chart/components/code-editor/json-editor.component";
import {ReportingCoreModule} from "@main/reporting.v3/reporting-core.module";
import {QueryBuilderModule} from "@main/reporting.v3/query-builder/query-builder.module";


@NgModule({
    declarations: [
        WidgetChartComponent,
        WidgetChartBuilderComponent,
        JsonEditorComponent
    ],
    providers: [
        PrismService
    ],
    exports: [
        WidgetChartComponent,
        WidgetChartBuilderComponent
    ],
    imports: [
        CommonModule,
        GridModule,
        PrimeNgModule,
        ColorPickerModule,

        ChartModule,
        QueryBuilderModule,
        ReportingCoreModule
    ]
})
export class WidgetChartModule {
}