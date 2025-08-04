import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

import { DbDocFormDirective } from "./shared/dbdoc-form.directive";
import { DbDocFormControlDirective } from "./shared/dbdoc-form-control.directive";
import { DbDocValidationErrorsComponent } from "./shared/dbdoc-validation-errors.component";


@NgModule({
    imports: [CommonModule],
    declarations: [DbDocFormDirective, DbDocFormControlDirective, DbDocValidationErrorsComponent],
    exports: [DbDocFormDirective, DbDocFormControlDirective, DbDocValidationErrorsComponent]
})
export class DbDocDirectivesModule {}