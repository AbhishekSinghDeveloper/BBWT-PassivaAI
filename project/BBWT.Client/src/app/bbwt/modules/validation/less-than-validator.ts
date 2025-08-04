import { Directive, Input } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, NgModel, ValidationErrors, Validator, ValidatorFn } from "@angular/forms";

import * as moment from "moment";


export function lessThanValidator(compareWith: NgModel | AbstractControl | string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        if (!compareWith) return null;

        const compareWithControl = (compareWith["control"] ?? control.parent.get(compareWith as string)) as AbstractControl;

        if (compareWithControl == null) return null;

        if (control.value == null) {
            if (control.hasError("lessThan")) {
                setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
            }

            return null;
        }

        const controlNumValue = Number(control.value);
        const controlMomentValue = moment(control.value);
        if (isNaN(controlNumValue) && !controlMomentValue.isValid()) {
            if (control.hasError("lessThan")) {
                setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
            }

            return null;
        }

        if (!isNaN(controlNumValue)) {
            if (compareWithControl["value"] != null) {
                const compareWithValue = Number(compareWithControl["value"]);
                if (compareWithValue != null && !isNaN(compareWithValue) && compareWithValue <= controlNumValue) {
                    if (!control.hasError("lessThan")) {
                        setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
                    }

                    return {
                        "lessThan": true
                    };
                }
            }
        }

        if (controlMomentValue.isValid()) {
            if (compareWithControl["value"] != null) {
                const compareWithValue = moment(compareWithControl["value"]);
                if (compareWithValue.isValid() && compareWithValue.toDate() <= controlMomentValue.toDate()) {
                    if (!control.hasError("lessThan")) {
                        setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
                    }

                    return {
                        "lessThan": true
                    };
                }
            }
        }


        if (control.hasError("lessThan")) {
            setTimeout(() => compareWithControl.updateValueAndValidity(), 10);
        }
        return null;
    };
}

@Directive({
    selector: "[lessThan][formControlName],[lessThan][formControl],[lessThan][ngModel]",
    providers: [{ provide: NG_VALIDATORS, useExisting: LessThanValidatorDirective, multi: true }]
})
export class LessThanValidatorDirective implements Validator {
    @Input() lessThan: NgModel | AbstractControl | string;

    validate(control: AbstractControl): ValidationErrors | null {
        return lessThanValidator(this.lessThan)(control);
    }
}