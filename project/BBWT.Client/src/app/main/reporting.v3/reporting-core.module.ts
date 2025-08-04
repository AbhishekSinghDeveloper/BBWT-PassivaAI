// Angular
import {NgModule, CUSTOM_ELEMENTS_SCHEMA} from "@angular/core";
import {CommonModule} from "@angular/common";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";

// 3d party
import {TreeTableModule} from "primeng/treetable";
import {PrimeNgModule} from "@primeng";
import {ChartModule} from "primeng/chart";


// BBWT
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";

import {GridModule} from "@features/grid";

import {QueryPreviewComponent} from "./components/query-preview.component";
import {QueryTableSelectorComponent} from "@main/reporting.v3/components/query-table-selector.component";
import {PdfExportingService} from "@main/reporting.v3/api/pdf-exporting.service";

@NgModule({
    declarations: [
        QueryPreviewComponent,
        QueryTableSelectorComponent
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        PrimeNgModule,
        TreeTableModule,
        ChartModule,

        // BBWT
        GridModule,
        BbwtSharedModule
    ],
    exports: [
        QueryPreviewComponent,
        QueryTableSelectorComponent
    ],
    providers: [PdfExportingService],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ReportingCoreModule {
}