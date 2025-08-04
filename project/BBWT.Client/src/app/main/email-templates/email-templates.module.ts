// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// NGX
import { QuillModule } from "ngx-quill";

// BBWT
import { EmailTemplateFormComponent } from "./email-template-form.component";
import { EmailTemplatesComponent } from "./email-templates.component";
import { EmailTemplateParameterService } from "./email-template-parameter.service";
import { EmailTemplateService } from "./email-template.service";
import { ServerValidatorDirective } from "./server-validator.directive";
import { GridModule } from "@features/grid";
import { BbCardModule } from "@features/bb-card";


const emailRoutes = [
    {
        path: "",
        component: EmailTemplatesComponent,
        data: { title: "Email Templates" }
    },
    {
        path: "edit/:id",
        component: EmailTemplateFormComponent,
        data: { title: "Email Template details" }
    },
];

@NgModule({
    declarations: [EmailTemplateFormComponent, EmailTemplatesComponent, ServerValidatorDirective],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        QuillModule, GridModule, BbCardModule,

        RouterModule.forChild(emailRoutes)
    ],
    exports: [],
    providers: [EmailTemplateParameterService, EmailTemplateService],
    bootstrap: [],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class EmailTemplatesModule { }