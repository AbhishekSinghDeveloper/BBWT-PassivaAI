import { ComponentCanDeactivate } from "./component-can-deactivate";
import { NgForm } from "@angular/forms";
import { Directive } from "@angular/core";

/*
To alert data loss in template driven form your component must extends this class.
In your component you must assign your local form variable (in template) to form property (with ViewChild).
Finally you must set the CanDeactivate guard to your component route.
*/
@Directive()
export abstract class TemplateDrivenFormCanDeactivate extends ComponentCanDeactivate {
    abstract get form(): NgForm;

    canDeactivate(): boolean {
        return this.form.submitted || !this.form.dirty;
    }
}