import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator, ValidatorFn } from "@angular/forms";
import { Directive } from "@angular/core";


export function noSideWhitespacesValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        const value = (control.value || "");
        return value == value.trim() ? null : {"noSideWhitespaces": true};
    };
}

@Directive({
    selector: "[noSideWhitespaces]",
    providers: [{ provide: NG_VALIDATORS, useExisting: NoSideWhitespacesValidatorDirective, multi: true }]
})
export class NoSideWhitespacesValidatorDirective implements Validator {
    validate(control: AbstractControl): ValidationErrors | null {
        return noSideWhitespacesValidator()(control);
    }
}