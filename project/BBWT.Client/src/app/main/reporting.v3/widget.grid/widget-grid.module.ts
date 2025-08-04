import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {PrimeNgModule} from "@primeng";

import {GridModule} from "@features/grid";
import {WidgetGridComponent} from "./components/widget-grid.component";
import {WidgetGridBuilderComponent} from "./components/widget-grid-builder.component";
import {WidgetGridBuilderService} from "./api/widget-grid-builder.service";
import {ReportingCoreModule} from "../reporting-core.module";
import {QueryBuilderModule} from "../query-builder/query-builder.module";
import {PaginatorModule} from "primeng/paginator";
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";
import { WidgetGridCustomFormatTooltipComponent } from "./components/widget-grid-custom-format-tooltip.component";
import { BbTooltipModule } from "@features/bb-tooltip";
import { CodeEditorModule } from "../code-editor/code-editor.module";


@NgModule({
    declarations: [WidgetGridComponent, WidgetGridBuilderComponent, WidgetGridCustomFormatTooltipComponent],
    exports: [WidgetGridComponent, WidgetGridBuilderComponent, WidgetGridCustomFormatTooltipComponent],
    imports: [
        CommonModule,
        GridModule,
        PrimeNgModule,
        ReportingCoreModule,
        QueryBuilderModule,
        PaginatorModule,
        BbwtSharedModule,
        BbTooltipModule,
        CodeEditorModule
    ],
    providers: [WidgetGridBuilderService]
})
export class WidgetGridModule {
}