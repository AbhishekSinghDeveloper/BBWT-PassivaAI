import {NgModule, CUSTOM_ELEMENTS_SCHEMA} from "@angular/core";
import {CommonModule} from "@angular/common";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";

import {QuillModule} from "ngx-quill";
import {PrimeNgModule} from "@primeng";
import {CodeEditorComponent} from "@main/reporting.v3/code-editor/components/code-editor.component";

@NgModule({
    declarations: [CodeEditorComponent],
    imports: [
        CommonModule,
        FormsModule,
        PrimeNgModule,
        ReactiveFormsModule,
        QuillModule.forRoot(),

        // BBWT
        BbwtSharedModule
    ],
    exports: [
        CodeEditorComponent
    ],
    providers: [],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class CodeEditorModule {
}