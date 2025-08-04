import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

// BB Card
import { BbCardComponent } from "./bb-card.component";
import { BbCardTitleComponent } from "./bb-card-title.component";
import { PrimeNgModule } from "@primeng";



@NgModule({
    imports: [
        CommonModule, PrimeNgModule
    ],
    declarations: [
        BbCardComponent, BbCardTitleComponent
    ],
    exports: [
        BbCardComponent
    ]
})
export class BbCardModule { }