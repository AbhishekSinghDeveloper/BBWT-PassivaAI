import { ComponentCanDeactivate } from "./component-can-deactivate";
import { FormGroup } from "@angular/forms";
import { Directive } from "@angular/core";

/*
To alert data loss in reactive form your component must extends this class.
In your component you must assign your form group instance to form property.
Furthermore, in your component your must define when your form is submitted or not.
Finally you must set the CanDeactivate guard to your component route.
 */
@Directive()
export abstract class ReactiveFormCanDeactivate extends ComponentCanDeactivate {
    abstract get form(): FormGroup;
    // Required because FormGroup instance doest not have submitted property unlike NgForm instance
    submitted = false;

    canDeactivate(): boolean {
        return this.submitted || !this.form.dirty;
    }
}