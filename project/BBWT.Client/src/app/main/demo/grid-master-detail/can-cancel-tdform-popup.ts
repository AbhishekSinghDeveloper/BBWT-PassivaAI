import { NgForm } from "@angular/forms";

export abstract class CanCancelTemplateDrivenFormPopup {
    popupVisible = false;

    abstract get form(): NgForm;

    onCancelPopup(): void {
        const { submitted, dirty } = this.form;
        if (!submitted && dirty) {
            this.popupVisible = !confirm("You have unsaved changes! If you leave, your changes will be lost.");
        } else {
            this.popupVisible = false;
        }
    }
}