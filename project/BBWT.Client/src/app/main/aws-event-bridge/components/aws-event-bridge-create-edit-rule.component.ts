import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from "@angular/core";
import {
    AbstractControl,
    UntypedFormBuilder,
    FormControl,
    UntypedFormGroup,
    ValidationErrors,
    Validators
} from "@angular/forms";

import { Observable, Subscription } from "rxjs";
import { debounceTime, filter, map, switchMap, tap } from "rxjs/operators";

import { awsCronValidator } from "../aws-cron-validator";
import { AwsEventBridgeJobInfo, AwsEventBridgeJobParameter, AwsEventBridgeRule } from "../dto";
import { AwsEventBridgeRuleService } from "../services";
import { AwsEventBridgeJobParametersComponent } from "./aws-event-bridge-job-parameters.component";


@Component({
    selector: "bbwt-aws-event-bridge-create-edit-rule",
    templateUrl: "./aws-event-bridge-create-edit-rule.component.html",
    styles: [
        `
            .eb-tooltip-label {
                vertical-align: top;
            }
        `
    ]
})
export class AwsEventBridgeCreateEditRuleComponent implements OnInit, OnDestroy {
    private static defaultRule: AwsEventBridgeRule = {
        isEnabled: true,
        parameters: []
    };

    private checkDuplicateRuleNameSubscription: Subscription;

    ruleForm: UntypedFormGroup = null;
    jobDescription: string = null;
    visible = true;
    availableTargets: AwsEventBridgeJobInfo[] = [];
    validatingNameAsync = false;

    @Input() rule: AwsEventBridgeRule;
    @Input() jobsInfo: AwsEventBridgeJobInfo[] = [];
    @ViewChild("jobParametersControl", { static: true }) jobsParametersControl: AwsEventBridgeJobParametersComponent;
    @Output()save = new EventEmitter<AwsEventBridgeRule>();
    @Output()cancel = new EventEmitter<any>();

    constructor(private fb: UntypedFormBuilder,
                private awsEventBridgeRuleService: AwsEventBridgeRuleService) {}


    get name() {
        return this.ruleForm && this.ruleForm.get("name");
    }

    get cron() {
        return this.ruleForm && this.ruleForm.get("cron");
    }

    get target() {
        return this.ruleForm && this.ruleForm.get("targetJobId");
    }

    get parameters() {
        return this.ruleForm && this.ruleForm.get("parameters");
    }

    get dialogWidth() {
        let minWidth = "460px";
        if (window.outerWidth <= 460) {
            minWidth = `${window.outerWidth}px`;
        }
        return {
            "min-width": minWidth
        };
    }

    private get selectedJob() {
        const jobId = this.target.value;
        const jobs = this.jobsInfo.filter((j) => j.jobId === jobId);
        return jobs && jobs.length && jobs[0];
    }



    private static getRequiredParameters(selectedJobInfo: AwsEventBridgeJobInfo, params: AwsEventBridgeJobParameter[]) {
        const defaultParam: AwsEventBridgeJobParameter = { name: null, value: null };
        const missing: AwsEventBridgeJobParameter[] = [];
        selectedJobInfo.parameters
            .filter((p) => p.required && !params.some((p2) => p2.name === p.name))
            .forEach((p) => missing.push({ ...defaultParam, name: p.name }));
        return [...missing, ...params];
    }


    ngOnInit(): void {
        const ruleTarget = this.rule && this.rule.targetJobId;
        this.availableTargets = this.jobsInfo.filter((j) => j.available || j.jobId === ruleTarget);

        const defaultTarget =
            this.availableTargets && this.availableTargets.length && this.availableTargets[0].jobId;
        const r = this.rule || {
            ...AwsEventBridgeCreateEditRuleComponent.defaultRule,
            targetJobId: defaultTarget
        };

        this.initForm(r);
        this.onTargetChange(true);

        this.checkDuplicateRuleNameSubscription = this.name.valueChanges
            .pipe(
                filter((ruleName: string) => ruleName && ruleName.length > 0),
                debounceTime(500),
                switchMap(ruleName => this.checkDuplicateRuleName(ruleName)))
            .subscribe(errors => this.name.setErrors(errors));
    }

    ngOnDestroy() {
        this.checkDuplicateRuleNameSubscription?.unsubscribe();
    }


    invalid(c: AbstractControl) {
        return c.invalid && (c.dirty || c.touched);
    }

    validateAndSave() {
        if (this.ruleForm.valid) {
            this.save.emit(this.ruleForm.value as AwsEventBridgeRule);
        } else {
            this.ruleForm.markAllAsTouched();
        }
    }

    onTargetChange(isInit = false) {
        const job = this.selectedJob;

        this.jobDescription = (job && job.jobDescription) || null;

        if (!isInit) {
            const requiredParams = AwsEventBridgeCreateEditRuleComponent.getRequiredParameters(
                job,
                []
            );
            this.jobsParametersControl.setTargetJobId(this.target.value);
            this.parameters.setValue(requiredParams);
        }
    }

    private checkDuplicateRuleName(ruleName: string): Observable<ValidationErrors | null> {
        const originalRuleName = ((this.rule && this.rule.name) || "").toLowerCase();
        ruleName = (ruleName ?? "").toLowerCase();
        this.validatingNameAsync = true;

        return this.awsEventBridgeRuleService
            .ruleExists(ruleName)
            .pipe(
                map((exists) =>
                    exists && ruleName != originalRuleName ? { duplicate: true } : null
                ),
                tap(() => this.validatingNameAsync = false)
            );
    }


    private initForm(rule: AwsEventBridgeRule) {
        this.ruleForm = this.fb.group({
            id: [rule.id],
            name: [rule.name, Validators.compose([Validators.required, Validators.pattern(/^[.\-_A-Za-z0-9]+$/)])],
            targetJobId: [rule.targetJobId, Validators.required],
            cron: [rule.cron, Validators.compose([Validators.required, awsCronValidator])],
            isEnabled: [rule.isEnabled],
            parameters: [[]]
        });

        const selectedJobInfo = this.jobsInfo.filter((info) => info.jobId === rule.targetJobId)[0];
        const params = AwsEventBridgeCreateEditRuleComponent.getRequiredParameters(
            selectedJobInfo,
            rule.parameters || []
        );
        this.jobsParametersControl.setJobsInfo(this.jobsInfo);
        this.jobsParametersControl.setTargetJobId(selectedJobInfo.jobId);
        this.parameters.setValue(params);
    }
}
