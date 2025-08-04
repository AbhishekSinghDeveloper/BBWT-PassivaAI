import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { PrimeNgModule } from "@primeng";

import { BbInputNumberRangeComponent } from "./bb-input-number-range.component";


@NgModule({
    imports: [CommonModule, FormsModule, ReactiveFormsModule, PrimeNgModule],
    declarations: [BbInputNumberRangeComponent],
    exports: [BbInputNumberRangeComponent]
})
export class BbInputNumberRangeModule {}