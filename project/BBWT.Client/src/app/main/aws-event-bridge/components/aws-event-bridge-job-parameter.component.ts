import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from "@angular/core";
import {
    AbstractControl,
    ControlValueAccessor,
    UntypedFormBuilder,
    UntypedFormGroup,
    NG_VALIDATORS,
    NG_VALUE_ACCESSOR,
    ValidationErrors,
    Validator
} from "@angular/forms";

import { Subscription } from "rxjs";

import { AwsEventBridgeJobParameter, AwsJobParameterInfo } from "../dto";
import { notEmptyValidator } from "@bbwt/modules/validation";


@Component({
    selector: "bbwt-aws-event-bridge-job-parameter",
    templateUrl: "./aws-event-bridge-job-parameter.component.html",
    styleUrls: ["./aws-event-bridge-job-parameter.component.scss"],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: AwsEventBridgeJobParameterComponent,
            multi: true
        },
        {
            provide: NG_VALIDATORS,
            useExisting: AwsEventBridgeJobParameterComponent,
            multi: true
        }
    ]
})
export class AwsEventBridgeJobParameterComponent implements ControlValueAccessor, Validator, OnInit, OnDestroy {
    f: UntypedFormGroup = null;
    param: AwsEventBridgeJobParameter = {
        name: "Unknown",
        value: null
    };
    
    paramName = "Unknown";

    @Output() remove = new EventEmitter<AwsEventBridgeJobParameter>();

    private changeSubs: Subscription[] = [];
    private _paramInfo: AwsJobParameterInfo = {
        name: "Unknown",
        required: false
    };

    constructor(private fb: UntypedFormBuilder) {}


    get invalid() {
        const v = this.f.get("val");
        return v.invalid && (v.touched || v.dirty);
    }

    @Input()
    set paramInfo(paramInfo: AwsJobParameterInfo) {
        this._paramInfo = { ...paramInfo };
        this.paramName = paramInfo.name;
        if (this.paramName.length > 10) {
            this.paramName = `${this.paramName.substring(0, 10)}...`;
        }
    }

    get paramInfo() {
        return this._paramInfo;
    }

    ngOnInit() {
        const validators = [];
        if (this.paramInfo.required) {
            validators.push(notEmptyValidator());
        }

        this.f = this.fb.group({
            val: [this.param.value, validators]
        });

        this.changeSubs.push(
            this.f.get("val").valueChanges.subscribe((v) => {
                this.param.value = v;
                this.onChange(this.param);
            })
        );
    }

    ngOnDestroy() {
        (this.changeSubs || []).forEach((s) => s.unsubscribe());
    }

    onChange(p: AwsEventBridgeJobParameter) {}

    onTouched() {}

    writeValue(obj: AwsEventBridgeJobParameter): void {
        this.param = obj;
        this.f.patchValue({
            val: obj.value
        });
    }

    registerOnChange(fn: (p: AwsEventBridgeJobParameter) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDisabledState(isDisabled: boolean): void {
        if (isDisabled) {
            this.f.get("val").disable();
        } else {
            this.f.get("val").enable();
        }
    }

    validate(control: AbstractControl): ValidationErrors | null {
        if (this.f.get("val").hasError("required")) {
            return { required: true };
        }
        return null;
    }


    onRemove() {
        this.remove.emit(this.param);
    }
}
