import {
    AbstractControl, AsyncValidator, AsyncValidatorFn, NG_ASYNC_VALIDATORS, ValidationErrors
} from "@angular/forms";
import { Directive, Input } from "@angular/core";

import { Observable } from "rxjs";

import { ReportService } from "./services/report.service";


export function reportAliasExistsAsyncValidator(service: ReportService, oldValue?: any): AsyncValidatorFn {
    return (control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> => {
        if (!control.value ||
            oldValue && (<string> control.value).toLowerCase() == (<string> oldValue).toLowerCase()) {
            return Promise.resolve(null);
        }

        return service.exists(control.value)
            .then(value => {
                if (value) {
                    Promise.resolve({reportExists: true} as ValidationErrors);
                } else {
                    Promise.resolve(null);
                }
            })
            .catch(() => Promise.resolve(null));
    };
}

@Directive({
    selector: "[reportAliasExistsAsync][formControlName],[reportAliasExistsAsync][formControl],[reportAliasExistsAsync][ngModel]",
    providers: [{ provide: NG_ASYNC_VALIDATORS, useExisting: ReportExistsAsyncValidatorDirective, multi: true }]
})
export class ReportExistsAsyncValidatorDirective implements AsyncValidator {
    @Input() reportAliasExistsAsyncOldValue: any;

    constructor(private reportService: ReportService) {}

    validate(control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> {
        return reportAliasExistsAsyncValidator(this.reportService, this.reportAliasExistsAsyncOldValue)(control);
    }
}