import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

import { FilterComponent } from "./filter.component";
import { FilterCustomTemplateDirective } from "./filter-custom-template.directive";
import { BbComboboxModule } from "../bb-combobox/bb-combobox.module";
import { BbInputNumberRangeModule } from "@features/bb-input-number-range";

@NgModule({
    declarations: [ FilterComponent, FilterCustomTemplateDirective ],
    exports: [ FilterComponent, FilterCustomTemplateDirective ],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule, BbComboboxModule, BbInputNumberRangeModule
    ]
})
export class FilterModule {}
