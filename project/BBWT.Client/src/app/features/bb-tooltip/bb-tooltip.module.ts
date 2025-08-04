import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

// BB Tooltip
import { BbTooltipComponent } from "./bb-tooltip.component";
import { TooltipModule } from "primeng/tooltip";

@NgModule({
    imports: [
        CommonModule, TooltipModule
    ],
    declarations: [
        BbTooltipComponent
    ],
    exports: [
        BbTooltipComponent
    ]
})
export class BbTooltipModule { }