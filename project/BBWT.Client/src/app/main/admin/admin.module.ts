import { CommonModule } from "@angular/common";
import { HttpClientJsonpModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { PrimeNgModule } from "@primeng";

import {
     DataAuditComponent, LoginAuditComponent
} from "./components";

import {
    DataAuditService, LoginAuditService
} from "./services";

import { JwtInterceptor } from "./interceptors/token.interceptor";
import { GridModule } from "@features/grid";
import { BbCardModule } from "@features/bb-card";

const adminRoutes = [
    { path: "login-audit", component: LoginAuditComponent, data: { title: "Login Audit" } },
    { path: "data-audit", component: DataAuditComponent, data: { title: "Data Audit" } }
];

@NgModule({
    declarations: [
        LoginAuditComponent, DataAuditComponent
    ],
    imports: [
        CommonModule, HttpClientJsonpModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        GridModule, BbCardModule,

        RouterModule.forChild(adminRoutes)
    ],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
        DataAuditService, LoginAuditService
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})

export class AdminModule {}