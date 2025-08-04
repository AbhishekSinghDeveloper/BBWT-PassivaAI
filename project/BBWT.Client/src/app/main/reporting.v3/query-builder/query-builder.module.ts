import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {PrimeNgModule} from "@primeng";
import {ReportingCoreModule} from "../reporting-core.module";
import {QueryBuilderComponent} from "./components/query-builder.component";
import {QueryAutoBuilderComponent} from "./components/query-auto-builder.component";
import {SqlEditorComponent} from "./components/sql-editor.component";
import {CodeEditorModule} from "../code-editor/code-editor.module";
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";


@NgModule({
    declarations: [
        SqlEditorComponent,
        QueryAutoBuilderComponent,
        QueryBuilderComponent
    ],
    exports: [QueryBuilderComponent],
    imports: [
        CommonModule,
        PrimeNgModule,
        ReportingCoreModule,
        CodeEditorModule,
        BbwtSharedModule
    ]
})
export class QueryBuilderModule {
}