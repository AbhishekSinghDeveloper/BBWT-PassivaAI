import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator, ValidatorFn } from "@angular/forms";
import { Directive, Input } from "@angular/core";


export function rangeDateValidator(min: Date, max: Date): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        const dateValue = control.value as Date;

        if (dateValue < min || dateValue > max) {
            return { "rangeDate": true };
        }

        return null;
    };
}

@Directive({
    selector: "[rangeDate][formControlName],[rangeDate][formControl],[rangeDate][ngModel]",
    providers: [{ provide: NG_VALIDATORS, useExisting: RangeDateValidatorDirective, multi: true }]
})
export class RangeDateValidatorDirective implements Validator {
    @Input() dateMinValue: Date;
    @Input() dateMaxValue: Date;

    validate(control: AbstractControl): ValidationErrors | null {
        return rangeDateValidator(this.dateMinValue, this.dateMaxValue)(control);
    }
}
