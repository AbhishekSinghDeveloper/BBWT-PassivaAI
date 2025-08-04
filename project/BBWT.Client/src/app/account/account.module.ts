
// BBWT
// Angular
import { CommonModule } from "@angular/common";
import { HttpClientJsonpModule } from "@angular/common/http";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { QRCodeModule } from "angularx-qrcode";
import { NgxCaptchaModule } from "ngx-captcha";
import { PrimeNgModule } from "@primeng";

import {
    AccountActivateComponent, AccountComponent, AccountRegistrationComponent, ConfirmEmailComponent,
    ForgotPasswordComponent, LoginComponent, ResetPasswordComponent
} from "./components/index";

import { AntiAutoCompleteDirective } from "./directives";
import { StrongPasswordModule } from "./modules/strong-password/strong-password.module";
import { SystemConfigurationResolver, SettingsSectionsName } from "@main/system-configuration";
import { ShowPasswordModule } from "@features/show-password/show-password.module";
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";


@NgModule({
    declarations: [
        AccountRegistrationComponent, ForgotPasswordComponent, LoginComponent, ResetPasswordComponent,
        AccountComponent,
        AntiAutoCompleteDirective,
        AccountActivateComponent,
        ConfirmEmailComponent
    ],
    imports: [
        CommonModule, HttpClientJsonpModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        ShowPasswordModule,
        BbwtSharedModule,

        // Zxcvbn
        StrongPasswordModule,
        // ReCaptcha
        NgxCaptchaModule,
        // QRCode
        QRCodeModule,

        // Routes
        RouterModule.forChild([{
            path: "",
            component: AccountComponent,
            children: [
                { path: "", redirectTo: "login", pathMatch: "full" },
                {
                    path: "login",
                    component: LoginComponent,
                    data: { title: "Log In", resolveSections: [SettingsSectionsName.UserPasswordSettings] }, // Pass config sections to be resolved for a component
                    resolve: { sysConfig: SystemConfigurationResolver }
                },
                {
                    path: "resetpassword",
                    component: ResetPasswordComponent,
                    data: { title: "Reset Password", resolveSections: [SettingsSectionsName.UserPasswordSettings] },
                    resolve: { sysConfig: SystemConfigurationResolver }
                },
                {
                    path: "activate",
                    component: AccountActivateComponent,
                    data: { title: "Account Activate", resolveSections: [SettingsSectionsName.UserPasswordSettings] },
                    resolve: { sysConfig: SystemConfigurationResolver }
                },
                {
                    path: "forgotpassword",
                    component: ForgotPasswordComponent,
                    data: { title: "Forgot Password" }
                },
                {
                    path: "register",
                    component: AccountRegistrationComponent,
                    data: { title: "Account Registration", resolveSections: [SettingsSectionsName.UserPasswordSettings, SettingsSectionsName.RegistrationSettings] },
                    resolve: { sysConfig: SystemConfigurationResolver }
                },
                {
                    path: "confirmemail",
                    component: ConfirmEmailComponent,
                    data: { title: "Confirm Email" }
                },
            ]
        }])
    ],
    bootstrap: [AccountComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AccountModule {
}