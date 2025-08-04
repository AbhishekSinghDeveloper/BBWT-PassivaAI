import { CommonModule } from "@angular/common";
import { HttpClientJsonpModule } from "@angular/common/http";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgIdleKeepaliveModule } from "@ng-idle/keepalive";

import { QRCodeModule } from "angularx-qrcode";
import { ClipboardModule } from "ngx-clipboard";
import { PrimeNgModule } from "@primeng";

import * as Quill from "quill";

import { MainRoutingModule } from "./main-routing.module";
import { CanDeactivateGuard } from "@bbwt/guards/can-deactivate.guard";
import { SecurityGuard } from "@bbwt/modules/security";

// BBWT Features references
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { DynamicFormModule } from "@features/dynamic-form";
import { BbImageUploaderModule } from "@features/bb-image-uploader";
import { BBWTTemplateMarker } from "@features/quill";
import { BbIconDropdownModule } from "@features/bb-icon-dropdown";

// BBWT Modules references
import { AdminGuard } from "./admin/guards/admin.guard";
import { AppLayoutModule } from "./app-layout";
import { RoleService, PermissionService, RolesDirectivesModule } from "./roles";
import { AwsStorageService } from "./aws-storage";
import {
    AwsEventBridgeRuleService,
    AwsEventBridgeRunningJobService,
    AwsEventBridgeSucceedJobService,
    AwsEventBridgeFailedJobService,
    AwsEventBridgeCanceledJobService,
    AwsEventBridgeJobService,
    AwsEventBridgeTechService
} from "./aws-event-bridge/services";
import { LoadingTimeService } from "./loading-time";
import { FooterMenuService, MainMenuService } from "./menu-designer";
import { StaticPageService } from "./static-pages";
import { AllowedIpService } from "./allowed-ip";
import { RuntimeEditorModule } from "./runtime-editor";

Quill.register("formats/bbwt-template-marker", BBWTTemplateMarker);

window["Quill"] = Quill;
require("quill-image-resize-module");

@NgModule({
    imports: [
        // Angular
        CommonModule,
        HttpClientJsonpModule,
        FormsModule,
        ReactiveFormsModule,
        ClipboardModule,
        NgIdleKeepaliveModule.forRoot(),
        PrimeNgModule,
        // QRCode
        QRCodeModule,
        // BBWT
        AppLayoutModule,
        BbCardModule,
        BbImageUploaderModule,
        BbIconDropdownModule,
        BbTooltipModule,
        DynamicFormModule,
        GridModule,
        FilterModule,
        RolesDirectivesModule,
        RuntimeEditorModule,
        // Routes
        MainRoutingModule
    ],
    providers: [
        CanDeactivateGuard,
        AdminGuard,
        AwsStorageService,
        AwsEventBridgeRuleService,
        AwsEventBridgeRunningJobService,
        AwsEventBridgeSucceedJobService,
        AwsEventBridgeCanceledJobService,
        AwsEventBridgeJobService,
        AwsEventBridgeTechService,
        AwsEventBridgeFailedJobService,
        AllowedIpService,
        MainMenuService,
        FooterMenuService,
        StaticPageService,
        SecurityGuard,
        RoleService,
        PermissionService,
        LoadingTimeService
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class MainModule {}
