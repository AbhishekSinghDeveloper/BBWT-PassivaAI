import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {PrimeNgModule} from "@primeng";
import {WidgetHtmlComponent} from "@main/reporting.v3/widget.html/components/widget-html.component";
import {WidgetHtmlBuilderComponent} from "@main/reporting.v3/widget.html/components/widget-html-builder.component";
import {ReportingCoreModule} from "@main/reporting.v3/reporting-core.module";
import {CodeEditorModule} from "@main/reporting.v3/code-editor/code-editor.module";


@NgModule({
    declarations: [WidgetHtmlComponent, WidgetHtmlBuilderComponent],
    exports: [WidgetHtmlComponent, WidgetHtmlBuilderComponent],
    imports: [CommonModule, PrimeNgModule, ReportingCoreModule, CodeEditorModule]
})
export class WidgetHtmlModule {
}