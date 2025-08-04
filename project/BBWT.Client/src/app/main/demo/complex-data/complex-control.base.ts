import { AfterViewInit, Injectable, OnDestroy } from "@angular/core";
import {
    AbstractControl,
    ControlValueAccessor,
    UntypedFormGroup,
    ValidationErrors,
    Validator
} from "@angular/forms";
import { Subscription } from "rxjs";

@Injectable()
export abstract class ComplexControlBase<TDTO>
    implements ControlValueAccessor, Validator, OnDestroy, AfterViewInit {
    complexForm = new UntypedFormGroup({});

    protected valueChangesSubscription: Subscription;

    constructor() {}

    invalid(c: AbstractControl) {
        return c.invalid && (c.dirty || c.touched);
    }

    // eslint-disable-next-line @angular-eslint/contextual-lifecycle
    ngAfterViewInit() {
        this.valueChangesSubscription = this.complexForm.valueChanges.subscribe(
            (orderDetails: TDTO) => this.onChange(orderDetails)
        );
    }

    ngOnDestroy() {
        this.valueChangesSubscription?.unsubscribe();
    }

    protected changeDisabledState(isDisabled: boolean, ...controls: AbstractControl[]) {
        controls.forEach(ctrl => (isDisabled ? ctrl.disable() : ctrl.enable()));
    }

    /**
     * ControlValueAccessor members
     */

    onChange(_orderDetails: TDTO) {}

    onTouched() {}

    writeValue(obj: TDTO): void {
        this.complexForm.patchValue(obj);
    }

    registerOnChange(fn: (obj: TDTO) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    /**
     * Validator members
     */

    validate(_control: AbstractControl): ValidationErrors | null {
        if (this.complexForm.invalid) {
            return {
                invalid: true
            };
        }

        return null;
    }
}
