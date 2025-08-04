// Angular
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

import { BbTooltipModule } from "@features/bb-tooltip";
import { NodeDialogComponent } from "./node-dialog.component";
import { RuntimeEditorComponent } from "./runtime-editor.component";


@NgModule({
    declarations: [
        RuntimeEditorComponent, NodeDialogComponent
    ],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        BbTooltipModule
    ],
    exports: [
        RuntimeEditorComponent
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class RuntimeEditorModule { }