import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

import { DynamicFormComponent } from "./dynamic-form.component";
import { GroupByPipe } from "./group-by.pipe";

@NgModule({
    declarations: [DynamicFormComponent, GroupByPipe],
    exports: [DynamicFormComponent],
    imports: [
        CommonModule, RouterModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule
    ]
})
export class DynamicFormModule { }