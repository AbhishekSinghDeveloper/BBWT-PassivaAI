import { Injectable, OnDestroy } from "@angular/core";

import { DeviceDetectorService } from "ngx-device-detector";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import { BroadcastService } from "../../bbwt/modules/broadcasting/broadcast.service";
import { CookieService } from "../cookie/cookie.service";
import { PwaSettings, SettingsSection, SettingsSectionsName, SystemConfigurationService } from "../system-configuration";


const PWA_INSTALL_PROMPT_CLOSED = "pwa-install-closed";

@Injectable({
    providedIn: "root"
})
export class PwaService implements OnDestroy {
    private _installationPromptEvent: any;
    private _appInstallationAvailable = false;
    private _isApplePwaCompatibleDevice = false;
    private _wasDeclined = false;
    private _isInstallationEnabled = false;
    private destroyed$ = new Subject<void>();


    constructor(private deviceDetectorService: DeviceDetectorService,
                private broadcastService: BroadcastService) {}


    get installationPromptEvent(): any {
        return this._installationPromptEvent;
    }

    get appInstallationAvailable(): boolean {
        return this._appInstallationAvailable;
    }

    get isApplePwaCompatibleDevice(): boolean {
        return this._isApplePwaCompatibleDevice;
    }

    get wasDeclined(): boolean {
        return this._wasDeclined;
    }

    get isInstallationEnabled(): boolean {
        return this._isInstallationEnabled;
    }


    ngOnDestroy(): void {
        this.destroyed$.next();
        this.destroyed$.complete();
    }

    initPwaPrompt() {
        this._wasDeclined = CookieService.GetCookie(PWA_INSTALL_PROMPT_CLOSED) === String(true);

        this._isApplePwaCompatibleDevice =
            this.deviceDetectorService.os === "Mac" && this.deviceDetectorService.browser !== "Safari"
            || this.deviceDetectorService.os === "iOS";

        if (!this._isApplePwaCompatibleDevice) {
            window.addEventListener("beforeinstallprompt", e => {
                e.preventDefault();
                this._installationPromptEvent = e;
                this._appInstallationAvailable = true;
            });

            window.addEventListener("appinstalled", () => {
                this._appInstallationAvailable = false;

                CookieService.SetPermanentCookie(PWA_INSTALL_PROMPT_CLOSED, String(false));
            });
        }

        this.broadcastService.on<SettingsSection>(SystemConfigurationService.SettingsSectionChangedEventName)
            .pipe(takeUntil(this.destroyed$))
            .subscribe(settingsSection => {
                if (settingsSection.sectionName != SettingsSectionsName.PwaSettings) return;
                this.handlePwaSettings(<PwaSettings>settingsSection.value);
            });
    }

    declinePrompt(): void {
        CookieService.SetPermanentCookie(PWA_INSTALL_PROMPT_CLOSED, String(true));
        this._wasDeclined = true;
    }


    private handlePwaSettings(pwaSettings: PwaSettings): void {
        this._isInstallationEnabled = !pwaSettings ||
            this.deviceDetectorService.isDesktop() && pwaSettings.desktopInstallationEnabled ||
            !this.deviceDetectorService.isDesktop() && pwaSettings.mobileInstallationEnabled;
    }
}