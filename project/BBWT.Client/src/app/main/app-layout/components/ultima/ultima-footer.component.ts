import { Component } from "@angular/core";

import { UltimaComponent } from "./ultima.component";
import { AppComponent } from "../app.component";


@Component({
    selector: "ultima-footer",
    templateUrl: "./ultima-footer.component.html",
    styleUrls: ["./ultima-footer.component.scss"]
})
export class UltimaFooterComponent {
    constructor(public app: AppComponent, public ultimaMain: UltimaComponent) {}
}