import { Component } from "@angular/core";

import { AppComponent } from "../app.component";
import { VeronaComponent } from "./verona.component";


@Component({
    selector: "verona-topbar",
    templateUrl: "./verona-topbar.component.html",
    styleUrls: ["./verona-topbar.component.scss"]
})
export class VeronaTopbarComponent {
    showRuntimeEditorSubMenu: boolean;

    constructor(public app: AppComponent, public veronaMain: VeronaComponent) { }

    onRuntimeEditorTopbarItemClick(event, rutimeEditorTopbarItem) {
        this.veronaMain.onTopbarItemClick(event, rutimeEditorTopbarItem);
        this.app.runtimeEditorToggle();
    }
}
