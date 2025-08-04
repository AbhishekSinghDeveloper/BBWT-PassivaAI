import {
    AbstractControl,
    NG_VALIDATORS,
    ValidationErrors,
    Validator,
    ValidatorFn
} from "@angular/forms";
import { Directive } from "@angular/core";


export function notEmptyValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        const isWhitespace = (control.value || "").trim().length === 0;
        const isValid = !isWhitespace;

        return isValid ? null : { "whitespace": true };
    };
}

@Directive({
    selector: "[notEmpty][formControlName],[notEmpty][formControl],[notEmpty][ngModel]",
    providers: [{ provide: NG_VALIDATORS, useExisting: NotEmptyValidatorDirective, multi: true }]
})
export class NotEmptyValidatorDirective implements Validator {
    validate(control: AbstractControl): ValidationErrors | null {
        return notEmptyValidator()(control);
    }
}