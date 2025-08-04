import { Component, OnDestroy, OnInit } from "@angular/core";
import {
    ControlValueAccessor,
    UntypedFormArray,
    UntypedFormBuilder,
    UntypedFormControl,
    UntypedFormGroup,
    NG_VALIDATORS,
    NG_VALUE_ACCESSOR,
    ValidationErrors,
    Validator
} from "@angular/forms";

import { Subscription } from "rxjs";

import { AwsEventBridgeJobInfo, AwsEventBridgeJobParameter, AwsJobParameterInfo } from "../dto";


interface MenuItemEx {
    label: string;
    command: (e: any) => void;
    paramInfo: AwsJobParameterInfo;
    disabled?: boolean;
}

@Component({
    selector: "bbwt-aws-event-bridge-job-parameters",
    templateUrl: "./aws-event-bridge-job-parameters.component.html",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: AwsEventBridgeJobParametersComponent,
            multi: true
        },
        {
            provide: NG_VALIDATORS,
            useExisting: AwsEventBridgeJobParametersComponent,
            multi: true
        }
    ]
})
export class AwsEventBridgeJobParametersComponent implements OnDestroy, ControlValueAccessor, Validator {
    pF: UntypedFormGroup = this.fb.group({
        parameters: this.fb.array([])
    });
    jobParametersItems: MenuItemEx[] = [];

    private jobsInfo: AwsEventBridgeJobInfo[] = [];
    private jobsParamsInfo: { [k: string]: AwsJobParameterInfo[] } = {};
    private targetJobId: string;
    private changeSubs: Subscription[] = [];


    constructor(private fb: UntypedFormBuilder) {}


    get parameters() {
        return this.pF && (this.pF.get("parameters") as UntypedFormArray);
    }

    get displayAddParameter() {
        return this.jobParametersItems.some((item) => !item.disabled);
    }


    onTouched() {}

    ngOnDestroy() {
        (this.changeSubs || []).forEach((subs) => subs.unsubscribe());
    }

    validate(control: UntypedFormControl): ValidationErrors | null {
        const value = (control.value as AwsEventBridgeJobParameter[]) || [];

        const allRequired = (this.jobsParamsInfo[this.targetJobId] || [])
            .filter((p) => p.required)
            .map((p) => p.name);
        const valuesRequired = value.some(
            (p) => allRequired.includes(p.name) && (p.value == null || p.value === "")
        );

        if (valuesRequired) {
            return { valuesRequired: true };
        }

        return null;
    }

    writeValue(obj: AwsEventBridgeJobParameter[]): void {
        this.parameters.clear();
        (obj || []).forEach((p) => this.parameters.push(this.fb.control(p)));

        const params = (obj || []).map((p) => p.name);
        this.jobParametersItems.forEach(
            (item) => (item.disabled = params.includes(item.paramInfo.name))
        );
    }

    registerOnChange(fn: any): void {
        this.changeSubs.push(this.parameters.valueChanges.subscribe(fn));
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDisabledState(isDisabled: boolean): void {
        if (isDisabled) {
            this.parameters.disable();
        } else {
            this.parameters.enable();
        }
    }


    /**
     * Sets the jobs info to be used by this component.
     *
     * @remarks This can be called once per-component instance,
     * should be called on the parent ngOnInit method and
     * before `@method setTargetJobId`
     *
     * @param jobsInfo The jobs info to be set
     */
    setJobsInfo(jobsInfo: AwsEventBridgeJobInfo[]) {
        this.jobsInfo = jobsInfo;
    }

    /**
     * Sets a new target job id to work with
     *
     * @remarks This should be called whenever the target job id
     * changes on the parent component.
     *
     * @param id The new target job id
     */
    setTargetJobId(id: string) {
        this.targetJobId = id;
        this.setupParamsInfo();
        this.jobParametersItems = (this.jobsParamsInfo[id] || [])
            .filter((p) => !p.required)
            .map(this.buildItem.bind(this));
    }

    getParamInfo(paramName: string) {
        const params = this.jobsParamsInfo[this.targetJobId];
        const paramInfo = params && params.filter((p) => p.name === paramName);
        return paramInfo && paramInfo.length && paramInfo[0];
    }

    removeParameter(param: AwsEventBridgeJobParameter) {
        this.onTouched();
        const index = this.parameters.controls.findIndex((c) => c.value === param);
        if (index >= 0) {
            this.parameters.removeAt(index);
            const item = this.jobParametersItems.filter((i) => i.paramInfo.name === param.name)[0];
            item.disabled = false;
        }
    }


    private buildItem(param: AwsJobParameterInfo) {
        return {
            label: `${param.name}`,
            command: (e: { item: MenuItemEx }) => this.addJobParameter(e.item),
            paramInfo: param
        };
    }

    private addJobParameter(item: MenuItemEx) {
        this.onTouched();
        const param: AwsEventBridgeJobParameter = {
            name: item.paramInfo.name,
            value: null
        };
        this.parameters.push(this.fb.control(param));
        item.disabled = true;
    }

    private setupParamsInfo() {
        this.jobsInfo.forEach((job) => {
            const source = {};
            source[job.jobId] = job.parameters;
            Object.assign(this.jobsParamsInfo, source);
        });
    }
}
