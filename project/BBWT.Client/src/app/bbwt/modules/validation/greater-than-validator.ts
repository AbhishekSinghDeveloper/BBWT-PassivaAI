import { Directive, Input } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, NgModel, ValidationErrors, Validator, ValidatorFn } from "@angular/forms";

import * as moment from "moment";


export function greaterThanValidator(compareWith: NgModel | AbstractControl | string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        if (!compareWith) return null;

        const compareWithControl = (compareWith["control"] ?? control.parent.get(compareWith as string)) as AbstractControl;

        if (compareWithControl == null) return null;

        if (control.value == null) {
            if (control.hasError("greaterThan")) {
                setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
            }

            return null;
        }

        const controlNumValue = Number(control.value);
        const controlMomentValue = moment(control.value);
        if (isNaN(controlNumValue) && !controlMomentValue.isValid()) {
            if (control.hasError("greaterThan")) {
                setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
            }

            return null;
        }

        if (!isNaN(controlNumValue)) {
            if (compareWithControl["value"] != null) {
                const compareWithValue = Number(compareWithControl["value"]);
                if (compareWithValue != null && !isNaN(compareWithValue) && compareWithValue >= controlNumValue) {
                    if (!control.hasError("greaterThan")) {
                        setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
                    }

                    return {
                        "greaterThan": true
                    };
                }
            }
        }

        if (controlMomentValue.isValid()) {
            if (compareWithControl["value"] != null) {
                const compareWithValue = moment(compareWithControl["value"]);
                if (compareWithValue.isValid() && compareWithValue.toDate() >= controlMomentValue.toDate()) {
                    if (!control.hasError("greaterThan")) {
                        setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
                    }

                    return {
                        "greaterThan": true
                    };
                }
            }
        }


        if (control.hasError("greaterThan")) {
            setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
        }
        return null;
    };
}

@Directive({
    selector: "[greaterThan][formControlName],[greaterThan][formControl],[greaterThan][ngModel]",
    providers: [{ provide: NG_VALIDATORS, useExisting: GreaterThanValidatorDirective, multi: true }]
})
export class GreaterThanValidatorDirective implements Validator {
    @Input() greaterThan: NgModel | AbstractControl | string;

    validate(control: AbstractControl): ValidationErrors | null {
        return greaterThanValidator(this.greaterThan)(control);
    }
}