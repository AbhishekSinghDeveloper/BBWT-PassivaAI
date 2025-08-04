import { Component, Input } from "@angular/core";
import { UntypedFormControl, UntypedFormGroup, AbstractControl } from "@angular/forms";

@Component({
    selector: "app-dynamic-form",
    templateUrl: "./dynamic-form.component.html",
    styleUrls: ["./dynamic-form.component.scss"]
})
export class DynamicFormComponent {
    @Input() set formGroup(value: UntypedFormGroup) {
        this._formGroup = value;
        this._initFormControls();
    }
    get formGroup(): UntypedFormGroup {
        return this._formGroup;
    }

    @Input() set formData(value: any) {
        this._formData = value;
        this._initFormControls();
    }
    get formData(): any {
        return this._formData;
    }

    @Input() subform = false;
    @Input() onSubmit: (object) => any;

    formControls: any[];

    private _formGroup: UntypedFormGroup;
    private _formData: any;

    constructor() {}

    // Form submit event
    submit() {
        if (typeof this.onSubmit === "undefined") {
            return;
        }

        // Execute user-specified submit method with form value object
        this.onSubmit(this.formGroup.value);
    }

    private _initFormControls() {
        this.formControls = [];
        for (const key in this.formGroup.controls) {
            if (!this.formGroup.controls.hasOwnProperty(key)) continue;

            const control = this.formGroup.controls[key];

            (<any>control).name = key;
            (<any>control).group = this._getGroup(control);

            this.formControls.push(control);
        }
    }

    private _getGroup(control: AbstractControl) {
        if (!this.formData || !this.formData.groups) {
            return "";
        }
        const group = this.formData.groups.find(x => x.controls.includes((<any>control).name));
        if (!group) {
            return "";
        }

        return group.title;
    }

    // Should the submit button be shown?
    shouldShowSubmit(): boolean {
        const isSubmitMethodDefined: boolean = typeof this.onSubmit !== "undefined";
        return !this.subform && isSubmitMethodDefined;
    }

    // Is the given control a control?
    isControl(control: AbstractControl): boolean {
        return control instanceof UntypedFormControl;
    }

    // Is the given control a group?
    isGroup(control: AbstractControl): boolean {
        return control instanceof UntypedFormGroup;
    }

    // Gets the respective formData entry for the control
    getControlData(control: AbstractControl): any {
        return this.formData[(control as any).name];
    }

    // Returns true if prompt should be shown on left of control (as normal)
    leftPrompt(control: AbstractControl): boolean {
        // Data object for this control
        const controlData: any = this.getControlData(control);

        // Use right prompt property if set
        if (typeof controlData.rightPrompt === "boolean") {
            return !controlData.rightPrompt;
        }

        // Default is left prompt
        return true;
    }

    paddingIsNotNeeded(control: AbstractControl) {
        const type = this.getControlType(control);
        switch (type) {
            case "group":
            case "radio":
                return true;
            default:
                return false;
        }
    }

    isGroupControl(control: AbstractControl) {
        const controlData = this.getControlData(control);
        if (controlData) {
            return controlData.type === "group";
        }
        return false;
    }

    // Executes the show method on a control's data object
    shouldShowRow(control: AbstractControl): boolean {
        const controlData = this.getControlData(control);

        // If this control has no show method, assume it should show
        if (controlData.show === undefined) {
            return true;
        }

        // Execute the show method, if it exists, with the current form value
        return controlData.show(this.formGroup.value);
    }

    // Returns the given control prompt
    getPrompt(control: AbstractControl): string {
        return this.getControlData(control).prompt || "";
    }

    // Returns the formatted prompt
    getFormattedPrompt(control: AbstractControl): string {
        const prompt = this.getPrompt(control);

        // Any characters after which we shouldn't add a colon
        const endings = ["?", ":", ";", ",", ".", "!", "-", "="];

        // Check if last character in prompt is one of above endings
        if (prompt == "" || !this.leftPrompt(control) || endings.indexOf(prompt.substr(-1)) !== -1) {
            // Use the prompt as-is
            return prompt;
        }

        // Otherwise add a colon to the end
        return prompt + ":";
    }

    // Returns the control type
    getControlType(control: AbstractControl): string {
        return this.getControlData(control).type;
    }

    // Returns the options array for a select element
    getOptions(control: AbstractControl): any[] {
        return this.getControlData(control).options;
    }

    // Returns the options array for a radio element
    getRadioOptions(control: AbstractControl): any[] {
        return this.getControlData(control).options;
    }

    // Returns the value property for the control
    getValue(control: AbstractControl): any {
        const controlData = this.getControlData(control);
        return controlData.value;
    }

    // Returns true if binary property set
    isBinary(control: AbstractControl): boolean {
        const controlData = this.getControlData(control);

        if ( typeof controlData.binary !== "undefined") {
            return controlData.binary;
        }

        // Default is binary
        return true;
    }

    // Returns the description for the given control, if any
    getDescription(control: AbstractControl): string {
        const controlData = this.getControlData(control);
        return controlData.description || "";
    }
}