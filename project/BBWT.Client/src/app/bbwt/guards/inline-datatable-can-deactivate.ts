import { ComponentCanDeactivate } from "./component-can-deactivate";
import { Directive } from "@angular/core";

/*
To alert data loss in inline form with primeng datatable your component must extends this class.
In your component's template you must map OnCellEditComplete to onEditComplete datatable event.
Furthermore, in your component your must define when your form is submitted or not.
Finally you must set the CanDeactivate guard to your component route.
 */
@Directive()
export abstract class InlineDatatableCanDeactivate extends ComponentCanDeactivate {
    submitted = false;
    dirty = false;

    onCellEditComplete(): void {
        this.dirty = true;
    }

    canDeactivate(): boolean {
        return this.submitted || !this.dirty;
    }
}