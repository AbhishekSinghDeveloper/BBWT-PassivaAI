// Angular
import { CommonModule } from "@angular/common";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { PrimeNgModule } from "@primeng";

// BBWT
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { DynamicFormModule } from "@features/dynamic-form";
import { FilterModule } from "@features/filter";
import { GridModule } from "@features/grid";
import { RolesDirectivesModule } from "@main/roles";
import { AllowedIpComponent } from "./components/allowed-ip.component";
import { AwsS3Component } from "./components/aws-s3.component";
import { EmailConfigurationComponent } from "./components/email-configuration.component";
import { FeedbackComponent } from "./components/feedback.component";
import { FormioSettingsComponent } from "./components/formio-settings.component";
import { LoginComponent } from "./components/login.component";
import { MaintenanceComponent } from "./components/maintenance.component";
import { ParametersStoreComponent } from "./components/parameters-store.component";
import { PasswordComponent } from "./components/password.component";
import { PerformanceComponent } from "./components/performance.component";
import { ProjectComponent } from "./components/project.component";
import { PwaSettingsComponent } from "./components/pwa-settings.component";
import { RegistrationComponent } from "./components/registration.component";
import { SessionComponent } from "./components/session.component";
import { FormioParameterListService } from "./formio-parameterlist.service";
import { ParametersStoreService } from "./parameters-store.service";
import { SystemConfigurationResolver } from "./system-configuration-resolver";
import { SystemConfigurationComponent } from "./system-configuration.component";


const routes = [
    {
        path: "",
        component: SystemConfigurationComponent,
        data: {
            title: "System Configuration"
        },
        resolve: { sysConfig: SystemConfigurationResolver }
    },
];

@NgModule({
    declarations: [
        SystemConfigurationComponent, EmailConfigurationComponent,
        PasswordComponent, LoginComponent, SessionComponent, RegistrationComponent, AwsS3Component,
        MaintenanceComponent, PerformanceComponent, ProjectComponent, AllowedIpComponent, FeedbackComponent,
        ParametersStoreComponent, PwaSettingsComponent, FormioSettingsComponent
    ],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,

        // BBWT
        GridModule, FilterModule, DynamicFormModule, BbCardModule, RolesDirectivesModule, BbTooltipModule,
        BbTooltipModule, BbwtSharedModule,

        RouterModule.forChild(routes)
    ],
    providers: [ParametersStoreService, FormioParameterListService],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SystemConfigurationModule { }