import { Component, Input } from "@angular/core";

import { DbDocFormDirective } from "./dbdoc-form.directive";


@Component({
    selector: "dbdoc-validation-errors",
    templateUrl: "./dbdoc-validation-errors.component.html"
})
export class DbDocValidationErrorsComponent {
    @Input() fieldName: string;

    constructor(public dbDocFormDirective: DbDocFormDirective) {}
}