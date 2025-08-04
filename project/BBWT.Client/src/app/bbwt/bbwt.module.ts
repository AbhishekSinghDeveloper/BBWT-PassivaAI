// Angular
import { CommonModule, registerLocaleData } from "@angular/common";
import { HttpClientJsonpModule, HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { CUSTOM_ELEMENTS_SCHEMA, ErrorHandler, NgModule, APP_INITIALIZER, LOCALE_ID, Injector } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule } from "@angular/router";
import { NgIdleKeepaliveModule } from "@ng-idle/keepalive";
import { MessageService, ConfirmationService } from "primeng/api";
import { ToastModule } from "primeng/toast";
import { ConfirmDialogModule } from "primeng/confirmdialog";

// BBWT Module
import { BBWTComponent } from "./components/bbwt.component";
import { SecurityGuard } from "./modules/security";
import { GlobalErrorHandler, CurrentUserWatchService } from "./modules/logging";
import {
    EmptyResponseBodyInterceptor, HttpResponsesHandlersFactory, HttpResponseSimulationInterceptor,
    HttpStatusesInterceptor, ApiInterceptor, DatesConversionInterceptor
} from "./modules/data-service";

// BBWT Modules references
import { AdminService } from "@main/admin/services";
import { ApiVersionInterceptor, ApiVersionService } from "@main/app-layout";
import { SystemConfigurationService, SystemConfigurationResolver } from "@main/system-configuration";
import { UserService } from "@main/users";
import { RoleService } from "@main/roles";
import { PwaModule } from "@main/pwa/pwa.module";
import { PwaService } from "@main/pwa/pwa.service";
import { BrowserInfoInterceptor } from "./modules/data-service/browser-info.interceptor";
import defaultLocale from "@angular/common/locales/en-GB";
import { ServiceLocator } from "./utils/ServiceLocator";

// FormIO
import { Formio, FormioAppConfig, FormioModule } from "@formio/angular";
import { FormioGrid } from "@formio/angular/grid";
import { FormioResources } from "@formio/angular/resource";
import { AppConfig } from "./config";
(Formio as any).icons = "fontawesome";

registerLocaleData(defaultLocale);


@NgModule({
    declarations: [
        BBWTComponent,
    ],
    imports: [
        CommonModule,
        BrowserModule,
        HttpClientModule,
        HttpClientJsonpModule,
        FormsModule,
        BrowserAnimationsModule,
        ReactiveFormsModule,
        ToastModule,
        ConfirmDialogModule,
        NgIdleKeepaliveModule.forRoot(),
        PwaModule,

        // FormIO
        FormioModule,
        FormioGrid,

        // Routes
        RouterModule.forRoot([
            { path: "account", loadChildren: () => import("../account/account.module").then(m => m.AccountModule) },
            {
                path: "app",
                loadChildren: () => import("../main/main.module").then(m => m.MainModule),
                canLoad: [SecurityGuard],
                resolve: { sysConfig: SystemConfigurationResolver }
            },
            { path: "**", redirectTo: "app" },
            { path: "", redirectTo: "app", pathMatch: "full" },
            { path: "saml", redirectTo: "saml" },
            { path: "saml/login", redirectTo: "saml/login" }
        ], { scrollPositionRestoration: "enabled" })
    ],
    providers: [
        AdminService,
        ApiVersionService,
        ConfirmationService,
        CurrentUserWatchService,
        HttpResponsesHandlersFactory,
        MessageService,
        SecurityGuard,

        // FormIO
        FormioResources,
        { provide: FormioAppConfig, useValue: AppConfig },

        { provide: LOCALE_ID, useValue: "en-GB" },

        { provide: HTTP_INTERCEPTORS, useClass: EmptyResponseBodyInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: HttpStatusesInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: ApiVersionInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: ApiInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: DatesConversionInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: HttpResponseSimulationInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: BrowserInfoInterceptor, multi: true },
        { provide: ErrorHandler, useClass: GlobalErrorHandler },

        {
            provide: APP_INITIALIZER,
            useFactory: (userService: UserService) => () => userService.initialize(),
            deps: [UserService],
            multi: true
        },
        {
            provide: APP_INITIALIZER,
            useFactory: (service: SystemConfigurationService) => () => service.initialize(),
            deps: [SystemConfigurationService],
            multi: true
        },
        {
            provide: APP_INITIALIZER,
            useFactory: (service: AdminService) => () => service.refreshCurrentUserAdminState(),
            deps: [AdminService],
            multi: true
        },
        {
            provide: APP_INITIALIZER,
            useFactory: (service: CurrentUserWatchService) => () => service.refreshLoggers(),
            deps: [CurrentUserWatchService],
            multi: true
        },
        {
            provide: APP_INITIALIZER,
            useFactory: (service: RoleService) => () => service.initialize(),
            deps: [RoleService],
            multi: true
        },
        {
            provide: APP_INITIALIZER,
            useFactory: (service: PwaService) => () => service.initPwaPrompt(),
            deps: [PwaService],
            multi: true
        }
    ],
    bootstrap: [BBWTComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class BBWTModule {
    constructor(private injector: Injector) {
        ServiceLocator.injector = this.injector;
    }
}
