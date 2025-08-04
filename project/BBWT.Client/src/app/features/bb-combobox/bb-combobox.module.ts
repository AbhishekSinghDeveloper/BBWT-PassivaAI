import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { BbComboboxComponent } from "./bb-combobox.component";
import { NgSelectModule } from "@ng-select/ng-select";

@NgModule({
    declarations: [
        BbComboboxComponent
    ],
    imports: [
        NgSelectModule,
        FormsModule
    ],
    exports: [BbComboboxComponent]
})
export class BbComboboxModule { }