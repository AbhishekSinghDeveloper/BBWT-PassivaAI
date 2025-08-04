// Angular
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import {
    ApiVersionComponent, AppComponent, AuthenticatorBannerComponent, IdleBannerComponent
} from "./components";
import { MaintenanceModule } from "@main/maintenance";
import { ApiVersionService } from "./api-version.service";
import { AppOfflineIndicatorComponent } from "../pwa/app-offline-indicator.component";
import { CookieModule } from "@main/cookie/cookie.module";
import { RuntimeEditorModule } from "../runtime-editor";
import {
    UltimaComponent,
    UltimaTopbarComponent,
    UltimaInlineProfileComponent,
    UltimaMenuComponent,
    UltimaMenuItemComponent,
    UltimaFooterComponent
} from "./components/ultima";
import {
    VeronaComponent,
    VeronaTopbarComponent,
    VeronaMenuComponent,
    VeronaMenuItemComponent,
    VeronaFooterComponent
} from "./components/verona";


@NgModule({
    declarations: [
        AppComponent,
        ApiVersionComponent,
        AuthenticatorBannerComponent,
        IdleBannerComponent,
        AppOfflineIndicatorComponent,
        UltimaComponent,
        UltimaTopbarComponent,
        UltimaInlineProfileComponent,
        UltimaMenuComponent,
        UltimaMenuItemComponent,
        UltimaFooterComponent,
        VeronaComponent,
        VeronaTopbarComponent,
        VeronaMenuComponent,
        VeronaMenuItemComponent,
        VeronaFooterComponent
    ],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,

        // BBWT
        MaintenanceModule,
        CookieModule,
        RuntimeEditorModule
    ],
    exports: [AppComponent],
    providers: [ApiVersionService],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppLayoutModule {}