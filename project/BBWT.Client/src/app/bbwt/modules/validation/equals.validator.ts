import { Directive, Input } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, NgModel, ValidationErrors, Validator, ValidatorFn } from "@angular/forms";


export function equalsValidator(to: NgModel | AbstractControl | string, deep?: boolean): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        if (!to) return null;

        const controlValue = control.value;

        let toControl = (to["control"] ?? to) as AbstractControl;
        let compareWithValue: any;
        if (to["value"]) {
            compareWithValue = to["value"];
        } else {
            toControl = control.parent.get(this.to as string);
            compareWithValue = toControl?.value;
        }

        if (!deep && controlValue != compareWithValue || deep && JSON.stringify(controlValue) != JSON.stringify(compareWithValue)) {
            if (control.hasError("equals")) {
                setTimeout(() => toControl.updateValueAndValidity(), 10);
            }

            return {
                "equals": true
            };
        } else {
            if (control.hasError("equals")) {
                setTimeout(() => toControl.updateValueAndValidity(), 10);
            }

            return null;
        }
    };
}

export function equalsToValueValidator(value: any, deep?: boolean): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null =>
        !deep && control.value == value || deep && JSON.stringify(control.value) == JSON.stringify(value)
            ? null
            : { "equalsToValue": true };
}

@Directive({
    selector: "[equals][formControlName],[equals][formControl],[equals][ngModel]",
    providers: [
        { provide: NG_VALIDATORS, useExisting: EqualValidatorDirective, multi: true }
    ]
})
export class EqualValidatorDirective implements Validator {
    @Input() equals: NgModel | AbstractControl | string;
    @Input() equalsToValue: any;
    @Input() equalsDeep: boolean;

    validate(c: AbstractControl): ValidationErrors | null {
        return typeof this.equalsToValue == "undefined"
            ? equalsValidator(this.equals, this.equalsDeep)(c)
            : equalsToValueValidator(this.equalsToValue, this.equalsDeep)(c);
    }
}