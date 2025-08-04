import { Component, ElementRef, ContentChild } from "@angular/core";
import { Card } from "primeng/card";

import { BbCardTitleComponent } from "./bb-card-title.component";

@Component({
    selector: "bb-card",
    templateUrl: "./bb-card.component.html",
    styleUrls: ["./bb-card.component.scss"]
})
export class BbCardComponent extends Card {
    @ContentChild(BbCardTitleComponent, { static: false }) titleContent;

    constructor(private elem: ElementRef) {
        super(elem);
    }
}
