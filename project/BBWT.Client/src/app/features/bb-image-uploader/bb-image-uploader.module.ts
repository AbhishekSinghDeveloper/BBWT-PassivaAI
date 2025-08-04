import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

import { BbImageUploaderComponent } from "./bb-image-uploader.component";
import { DropZoneDirective } from "./dropZone.directive";

@NgModule({
    declarations: [BbImageUploaderComponent, DropZoneDirective],
    exports: [BbImageUploaderComponent],
    imports: [
        CommonModule, RouterModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule
    ]
})
export class BbImageUploaderModule { }
