import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from "@angular/forms";
import { ICustomer } from "../northwind";
import { ComplexControlBase } from "./complex-control.base";

@Component({
    selector: "customer-input",
    templateUrl: "./customer-form.component.html",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: CustomerFormComponent,
            multi: true
        },
        {
            provide: NG_VALIDATORS,
            useExisting: CustomerFormComponent,
            multi: true
        }
    ]
})
export class CustomerFormComponent extends ComplexControlBase<ICustomer> implements OnInit {
    constructor(private formBuilder: UntypedFormBuilder) {
        super();
    }

    get code() {
        return this.complexForm?.get("code");
    }

    get companyName() {
        return this.complexForm?.get("companyName");
    }

    ngOnInit() {
        this.complexForm = this.formBuilder.group({
            code: [null, Validators.required],
            companyName: [null, Validators.required]
        });
    }

    setDisabledState(isDisabled: boolean) {
        this.changeDisabledState(isDisabled, this.code, this.companyName);
    }
}
