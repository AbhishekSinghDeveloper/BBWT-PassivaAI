import { Component, Input } from "@angular/core";

@Component({
    selector: "bb-tooltip",
    templateUrl: "./bb-tooltip.component.html",
    styleUrls: ["./bb-tooltip.component.scss"]
})
export class BbTooltipComponent {
    @Input() message: string;

    constructor() {}
}