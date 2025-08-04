import { Component } from "@angular/core";

import { AppComponent } from "../app.component";
import { UltimaComponent } from "./ultima.component";


@Component({
    selector: "ultima-topbar",
    templateUrl: "./ultima-topbar.component.html",
    styleUrls: ["./ultima-topbar.component.scss"]
})
export class UltimaTopbarComponent {
    constructor(public app: AppComponent, public ultimaMain: UltimaComponent) { }

    onRuntimeEditorTopbarItemClick(event, rutimeEditorTopbarItem) {
        if (this.app.runtimeEditorToggle()) {
            this.ultimaMain.onTopbarItemClick(event, rutimeEditorTopbarItem);
        } else {
            event.preventDefault();
        }
    }
}
