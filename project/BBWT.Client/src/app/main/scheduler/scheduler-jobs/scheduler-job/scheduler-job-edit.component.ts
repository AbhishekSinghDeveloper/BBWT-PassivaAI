import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { AbstractControl, UntypedFormBuilder, UntypedFormGroup, ValidationErrors, Validators } from "@angular/forms";
import { JobExecutionDetails } from "@main/scheduler/JobExecutionDetails";
import { SchedulerService } from "@main/scheduler/scheduler.service";
import { debounceTime, filter, map, Observable, Subscription, switchMap, tap } from "rxjs";

@Component({
  selector: "scheduler-job-edit",
  templateUrl: "./scheduler-job-edit.component.html",
  styleUrls: ["./scheduler-job-edit.component.scss"]
})
export class SchedulerJobEditComponent implements OnInit {
  private static defaultRule: JobExecutionDetails = {
    isEnabled: true
  };
  @Output() cancel = new EventEmitter<any>();
  @Input() rule: JobExecutionDetails;
  @Input() jobsInfo: JobExecutionDetails;
  @Output() save = new EventEmitter<JobExecutionDetails>();

  availableTargets: JobExecutionDetails[] = [];
  private checkDuplicateRuleNameSubscription: Subscription;
  constructor(private fb: UntypedFormBuilder, private schedulerService: SchedulerService) {
  }

  get name() {
    return this.ruleForm && this.ruleForm.get("jobName");
  }

  get cron() {
    return this.ruleForm && this.ruleForm.get("cron");
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

  ngOnInit(): void {
    const ruleTarget = this.rule && this.rule.id;
  
    if (this.jobsInfo) {
      this.availableTargets = [this.jobsInfo];
    }
  
    const defaultTarget =
      this.availableTargets && this.availableTargets.length && this.availableTargets[0].id;
  
    const r = this.rule || {
      ...SchedulerJobEditComponent.defaultRule,
      targetJobId: defaultTarget
    };
  
    this.initForm(r);
  
    this.checkDuplicateRuleNameSubscription = this.name.valueChanges
      .pipe(
        filter((ruleName: string) => ruleName && ruleName.length > 0),
        debounceTime(500),
        switchMap(ruleName => this.checkDuplicateRuleName(ruleName)))
      .subscribe(errors => this.name.setErrors(errors));
  }
  
  private checkDuplicateRuleName(ruleName: string): Observable<ValidationErrors | null> {
    const originalRuleName = ((this.rule && this.rule.jobName) || "").toLowerCase();
    ruleName = (ruleName ?? "").toLowerCase();
    this.validatingNameAsync = true;

    return this.schedulerService
      .ruleExists(ruleName)
      .pipe(
        map((exists) =>
          exists && ruleName != originalRuleName ? { duplicate: true } : null
        ),
        tap(() => this.validatingNameAsync = false)
      );
  }
  ruleForm: UntypedFormGroup = null;
  jobDescription: string = null;

  visible = true;
  validatingNameAsync = false;



  invalid(c: AbstractControl) {
    return c.invalid && (c.dirty || c.touched);
  }

  validateAndSave() {
    if (this.ruleForm.valid) {
      this.save.emit(this.ruleForm.value as JobExecutionDetails);
    } else {
      this.ruleForm.markAllAsTouched();
    }
  }

  quartzCronValidator(control: AbstractControl): ValidationErrors | null {
    const cronPattern = 
    /^((((\d+,)+\d+|(\d+(\/|-|#)\d+)|\d+L?|\*(\/\d+)?|L(-\d+)?|\?|[A-Z]{3}(-[A-Z]{3})?) ?){5,7})$/;
    if (!cronPattern.test(control.value)) {
      return { invalidCron: true };
    }
  
    return null;
  }
  
  private initForm(rule: JobExecutionDetails) {
    const defaultJobName = this.jobsInfo ? this.jobsInfo.jobName : "";
    const defaultCron = this.jobsInfo ? this.jobsInfo.cron : "";
    this.ruleForm = this.fb.group({
      id: [rule.id],
      jobName: [
        rule.jobName || defaultJobName, 
        Validators.compose([
          Validators.required, 
          Validators.pattern(/^[.\-_A-Za-z0-9]+$/)
        ])
      ],
      cron: [
        rule.cron || defaultCron, 
        Validators.compose([
          Validators.required, 
          this.quartzCronValidator
        ])
      ],
      isEnabled: [rule.isEnabled],
      parameters: [[]]
    });
  }
  
}