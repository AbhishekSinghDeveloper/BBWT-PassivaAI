import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";

// BB Card
import { BbIconDropdownComponent } from "./bb-icon-dropdown.component";
import { PrimeNgModule } from "@primeng";

@NgModule({
    imports: [
        CommonModule, FormsModule, PrimeNgModule
    ],
    declarations: [
        BbIconDropdownComponent
    ],
    exports: [
        BbIconDropdownComponent
    ]
})
export class BbIconDropdownModule {}