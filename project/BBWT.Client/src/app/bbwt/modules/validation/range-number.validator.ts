import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator, ValidatorFn } from "@angular/forms";
import { Directive, Input } from "@angular/core";
import { IHash } from "../../interfaces";


export function rangeNumberValidator(min: number, max: number): ValidatorFn {
    return (control: AbstractControl): IHash => {
        const numberValue = control.value as number;

        if (numberValue < min || numberValue > max) {
            return { "rangeNumber": true };
        }

        return null;
    };
}

@Directive({
    selector: "[rangeNumber][formControlName],[rangeNumber][formControl],[rangeNumber][ngModel]",
    providers: [{ provide: NG_VALIDATORS, useExisting: RangeNumberValidatorDirective, multi: true }]
})
export class RangeNumberValidatorDirective implements Validator {
    @Input() numberMinValue: number;
    @Input() numberMaxValue: number;

    validate(control: AbstractControl): ValidationErrors | null {
        return rangeNumberValidator(this.numberMinValue, this.numberMaxValue)(control);
    }
}