import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { PrimeNgModule } from "@primeng";

import { GridComponent } from "./grid.component";
import { GridColumnTemplate } from "./grid-template.directive";
import { GridEditingDialogComponent } from "./grid-editing-dialog.component";
import { GridImportDialogComponent } from "./grid-import-dialog.component";


@NgModule({
    declarations: [ GridComponent, GridColumnTemplate, GridEditingDialogComponent, GridImportDialogComponent ],
    exports: [ GridComponent, GridColumnTemplate ],
    imports: [
        CommonModule, RouterModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule
    ]
})
export class GridModule {}
