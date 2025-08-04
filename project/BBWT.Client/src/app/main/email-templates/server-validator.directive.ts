import { HttpClient, HttpParams } from "@angular/common/http";
import { Directive, Input } from "@angular/core";
import { AbstractControl, NG_ASYNC_VALIDATORS, Validator } from "@angular/forms";
import { Observable, Subscription } from "rxjs";
import { IHash } from "@bbwt/interfaces";

@Directive({
    selector: "[serverValidate][formControlName],[serverValidate][formControl],[serverValidate][ngModel]",
    providers: [
        { provide: NG_ASYNC_VALIDATORS, useExisting: ServerValidatorDirective, multi: true }
    ]
})
export class ServerValidatorDirective implements Validator {
    timer: any = null;
    request: Subscription = null;
    @Input() serverValidate: string;
    @Input() queryParams: any;

    constructor(private http: HttpClient) { }

    validate(c: AbstractControl): Promise<IHash> | Observable<IHash> {
        return new Promise<IHash>(resolve => {
            if (this.timer) {
                clearTimeout(this.timer);
                this.timer = null;
            }

            if (this.request) {
                this.request.unsubscribe();
                this.request = null;
            }

            if (c.value) {
                this.timer = setTimeout(() => {
                    this.timer = null;
                    let params = new HttpParams();
                    params = params.set("value", c.value);
                    Object.keys(this.queryParams).forEach(key => {
                        params = params.set(key, this.queryParams[key]);
                    });

                    this.request = this.http.get(this.serverValidate, { responseType: "text", params: params }).subscribe(value => {
                        resolve(value === "true" ? null : {
                            serverValidate: true
                        });
                    });
                }, 500);
            }
        });
    }
}