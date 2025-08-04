import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from "@angular/forms";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { IEmployee } from "../northwind";
import { ComplexControlBase } from "./complex-control.base";

@Component({
    selector: "employee-input",
    templateUrl: "./employee-form.component.html",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: EmployeeFormComponent,
            multi: true
        },
        {
            provide: NG_VALIDATORS,
            useExisting: EmployeeFormComponent,
            multi: true
        }
    ]
})
export class EmployeeFormComponent extends ComplexControlBase<IEmployee> implements OnInit {
    constructor(private formBuilder: UntypedFormBuilder) {
        super();
    }

    get name() {
        return this.complexForm?.get("name");
    }

    get age() {
        return this.complexForm?.get("age");
    }

    get phone() {
        return this.complexForm?.get("phone");
    }

    get email() {
        return this.complexForm?.get("email");
    }

    get jobRole() {
        return this.complexForm?.get("jobRole");
    }

    ngOnInit() {
        this.complexForm = this.formBuilder.group({
            name: [null, [Validators.required]],
            age: [null, [Validators.required, Validators.pattern(/[1-9]\d*/)]],
            phone: [null, [Validators.required, Validators.pattern(ValidationPatterns.phone)]],
            email: [null, [Validators.required, Validators.pattern(ValidationPatterns.email)]],
            jobRole: [null, Validators.required]
        });
    }

    setDisabledState(isDisabled: boolean) {
        this.changeDisabledState(
            isDisabled,
            this.name,
            this.age,
            this.phone,
            this.email,
            this.jobRole
        );
    }
}
