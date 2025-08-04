import { Directive, Input } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator } from "@angular/forms";

import { DbDocFormDirective } from "./dbdoc-form.directive";


@Directive({
    selector: "[dbDocControl]",
    providers: [{ provide: NG_VALIDATORS, useExisting: DbDocFormControlDirective, multi: true }]
})
export class DbDocFormControlDirective implements Validator {
    @Input("dbDocControl") fieldName: string;

    constructor(private dbDocFormDirective: DbDocFormDirective) {}

    validate(control: AbstractControl): ValidationErrors | null {
        if (!this.dbDocFormDirective) return null;

        const validators = this.dbDocFormDirective.getValidatorsForField(this.fieldName);

        if (!validators?.length) return null;

        let result = null;
        validators.forEach(validator => {
            const validationResult = validator(control);

            if (validationResult) {
                if (!result) result = {};
                Object.keys(validationResult).forEach(key => result[key] = validationResult[key]);
            }
        });

        return result;
    }
}