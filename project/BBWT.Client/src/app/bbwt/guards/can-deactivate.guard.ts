import { Injectable } from "@angular/core";

import { ComponentCanDeactivate } from "./component-can-deactivate";

@Injectable()
export class CanDeactivateGuard  {
    canDeactivate(component: ComponentCanDeactivate): boolean {
        if (!component.canDeactivate()) {
            return confirm("You have unsaved changes! If you leave, your changes will be lost.");
        }

        return true;
    }
}