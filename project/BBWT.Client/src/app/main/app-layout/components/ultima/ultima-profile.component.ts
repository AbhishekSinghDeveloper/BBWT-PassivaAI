import { Component } from "@angular/core";
import { trigger, state, transition, style, animate } from "@angular/animations";

import { AppComponent } from "../app.component";


@Component({
    selector: "ultima-inline-profile",
    templateUrl: "./ultima-profile.component.html",
    animations: [
        trigger("menu", [
            state("hidden", style({
                height: "0px"
            })),
            state("visible", style({
                height: "*"
            })),
            transition("visible => hidden", animate("400ms cubic-bezier(0.86, 0, 0.07, 1)")),
            transition("hidden => visible", animate("400ms cubic-bezier(0.86, 0, 0.07, 1)"))
        ])
    ]
})
export class UltimaInlineProfileComponent {
    active: boolean;

    constructor(public app: AppComponent) {}

    onClick(event): void {
        this.active = !this.active;
        event.preventDefault();
    }
}
