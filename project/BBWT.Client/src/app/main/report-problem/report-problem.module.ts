// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import { ReportProblemComponent } from "./report-problem.component";
import { BbCardModule } from "@features/bb-card";

const routes = [
    {
        path: "",
        component: ReportProblemComponent,
        data: { title: "Report Problem" }
    }
];

@NgModule({
    declarations: [ReportProblemComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        BbCardModule,

        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ReportProblemModule { }