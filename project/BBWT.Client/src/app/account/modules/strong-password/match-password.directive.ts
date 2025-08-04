import { Directive, Input, OnChanges, SimpleChanges } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, Validator, ValidatorFn, Validators } from "@angular/forms";
import { IHash } from "@bbwt/interfaces";

export function MatchPasswordValidator(password: string): ValidatorFn {
    return (control: AbstractControl): IHash => {
        return control.value !== password ? {
            "match": {
                valid: false
            }
        } : null;
    };
}

@Directive({
    selector: "[match]",
    providers: [{ provide: NG_VALIDATORS, useExisting: MatchPasswordValidatorDirective, multi: true }]
})
export class MatchPasswordValidatorDirective implements Validator, OnChanges {
    @Input() match: string;
    control: AbstractControl;
    private validatorFn = Validators.nullValidator;

    ngOnChanges(changes: SimpleChanges): void {
        const change = changes["match"];
        if (change) {
            this.validatorFn = MatchPasswordValidator(change.currentValue);
            if (this.control) this.control.updateValueAndValidity();
        } else {
            this.validatorFn = Validators.nullValidator;
        }
    }

    validate(control: AbstractControl): IHash {
        this.control = control;
        return this.validatorFn(control);
    }
}