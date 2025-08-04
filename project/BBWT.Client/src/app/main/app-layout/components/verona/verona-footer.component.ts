import { Component } from "@angular/core";

import { VeronaComponent } from "./verona.component";
import { AppComponent } from "../app.component";


@Component({
    selector: "verona-footer",
    styleUrls: ["./verona-footer.component.scss"],
    templateUrl: "./verona-footer.component.html"
})
export class VeronaFooterComponent {
    constructor(public app: AppComponent, public veronaMain: VeronaComponent) {}
}
