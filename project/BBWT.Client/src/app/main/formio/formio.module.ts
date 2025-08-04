// Angular
import { NgModule } from "@angular/core";
import { RouterModule, mapToCanActivate } from "@angular/router";
import { PrimeNgModule } from "@primeng";
// BBWT

import { FilterModule } from "@features/filter";
import { GridModule } from "@features/grid";
import { FormIOBuilderComponent } from "./components/formio-builder/formio-builder.component";
import { FormIODisplayComponent } from "./components/formio-display/formio-display.component";
import { FormIOInstancesComponent } from "./components/formio-instances/formio-instances.component";
import { FormIOListComponent } from "./components/formio-list/formio-list.component";
import { FormioPDFGeneratorComponent } from "./components/formio-pdf/formio-pdf-generator.component";

import { BbFormIOModule } from "@features/bb-formio";
import { FormioDataViewerService } from "./services/formioDataViewer.service";
import { FormioViewerService } from "./services/formioViewer.service";
import { FormIODataExplorerComponent } from "./components/formio-data-explorer/formio-data-explorer.component";
import { FormTreeViewComponent } from "./components/formio-data-explorer/form-tree-view/form-tree-view.component";
import { FormioDataGridComponent } from "./components/formio-data-explorer/formio-data-grid/formio-data-grid.component";
import { DataExplorerCustomGridComponent } from "./components/formio-data-explorer/data-explorer-custom-grid/data-explorer-custom-grid.component";
import { FormioGuard } from "./guards/can-activate-formio";
import { FormIODisabledComponent } from "./components/formio-disabled/formio-disabled.component";
import { FormIODetailsComponent } from "./components/formio-details/formio-details.component";
import { FormIORequestComponent } from "./components/formio-request/formio-request.component";
import { FormioRequestService } from "./services/formioRequest.service";
import { FormioSurveysListComponent } from "./components/formio-surveys/formio-surveys-list/formio-surveys-list.component";
import { FormioSurveysPendingComponent } from "./components/formio-surveys/formio-surveys-pending/formio-surveys-pending.component";
import { FormIOSurveyService } from "./services/formioSurvey.service";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { FormioPendingSurveysService } from "./services/formioPendingSurveys.service";
import { FormIOFormCategoryComponent } from "./components/form-categories/form-categories.component";
import { FormIOCategoryService } from "./services/formioCategory.service";
import { FormioAllInstancesComponent } from "./components/formio-all-instances/formio-all-instances.component";


const routes = [
    {
        path: "",
        children: [
            { path: "multiuser", loadChildren: () => import("./multi-user-form/multi-user-form.module").then(m => m.MultiFormModule) }
        ]
    },
    {
        path: "disabled",
        component: FormIODisabledComponent,
        data: { title: "Forms Feature Disabled" },
    },
    {
        path: "list",
        component: FormIOListComponent,
        data: { title: "Form Designs" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "categories",
        component: FormIOFormCategoryComponent,
        data: { title: "Form Categories" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "form-data-explorer",
        component: FormIODataExplorerComponent,
        data: { title: "Form Data Explorer" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "builder",
        component: FormIOBuilderComponent,
        data: { title: "Form Builder" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "pdf",
        component: FormioPDFGeneratorComponent,
        data: { title: "Form PDF Generator" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "display",
        component: FormIODisplayComponent,
        data: { title: "Form Display" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "instances",
        component: FormIOInstancesComponent,
        data: { title: "Form Instances" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "details",
        component: FormIODetailsComponent,
        data: { title: "Form Details" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "survey-list",
        component: FormioSurveysListComponent,
        data: {title: "Surveys"},
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "survey-pending",
        component: FormioSurveysPendingComponent,
        data: { title: "Pending Surveys" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "form-instances-explorer",
        component: FormioAllInstancesComponent,
        data: { title: "Form Instances Explorer" },
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "requests",
        component: FormIORequestComponent,
        data: { title: "Form Requests" },
        canActivate: mapToCanActivate([FormioGuard]),
    }
];

@NgModule({
    declarations: [
        FormIOListComponent,
        FormIOBuilderComponent,
        FormioPDFGeneratorComponent,
        FormIODisplayComponent,
        FormIOInstancesComponent,
        FormIODataExplorerComponent,
        FormTreeViewComponent,
        FormioDataGridComponent,
        DataExplorerCustomGridComponent,
        FormIODisabledComponent,
        FormIODetailsComponent,
        FormIORequestComponent,
        FormioSurveysListComponent,
        FormioSurveysPendingComponent,
        FormIOFormCategoryComponent,
        FormioAllInstancesComponent
    ],
    imports: [
        GridModule,
        FilterModule,
        FormsModule,
        ReactiveFormsModule,
        // BB Formio Module
        BbFormIOModule,
        PrimeNgModule,

        RouterModule.forChild(routes),
    ],
    providers: [
        FormioGuard,
        FormioViewerService,
        FormioDataViewerService,
        FormioRequestService,
        FormIOSurveyService,
        FormioPendingSurveysService,
        FormIOCategoryService
    ]
})
export class FormIOModule { }