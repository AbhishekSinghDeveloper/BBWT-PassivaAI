import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { ServiceWorkerModule, SwRegistrationOptions } from "@angular/service-worker";

import { SidebarModule } from "primeng/sidebar";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";

import { environment } from "@environments/environment";
import { PwaComponent } from "./pwa.component";
import { AppInstallComponent } from "./app-install.component";
import { AppAutoUpdateComponent } from "./app-auto-update.component";
import { AppOnlineStateService } from "./app-online-state.service";
import { SystemConfigurationService } from "../system-configuration/system-configuration.service";


@NgModule({
    declarations: [AppInstallComponent, AppAutoUpdateComponent, PwaComponent],
    imports: [
        CommonModule,
        BrowserModule,
        BrowserAnimationsModule,
        SidebarModule,
        ButtonModule,
        TooltipModule,

        ServiceWorkerModule.register("ngsw-worker.js")
    ],
    providers: [
        {
            provide: SwRegistrationOptions,
            useFactory: (systemConfigurationService: SystemConfigurationService) => ({
                enabled: environment.production && systemConfigurationService.pwaEnabled,
                registrationStrategy: "registerImmediately"
            }),
            deps: [SystemConfigurationService]
        },
        AppOnlineStateService
    ],
    exports: [PwaComponent]
})
export class PwaModule {}
