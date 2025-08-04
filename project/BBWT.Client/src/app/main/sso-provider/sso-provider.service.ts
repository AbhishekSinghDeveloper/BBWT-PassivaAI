import { Injectable, Inject } from "@angular/core";
import { DOCUMENT } from "@angular/common";

import {
    GoogleSsoSettings,
    LinkedInSsoSettings,
    FacebookSsoSettings,
    SystemConfigurationService,
    SettingsSectionsName
} from "@main/system-configuration";
import { CookieService } from "@main/cookie/cookie.service";


@Injectable({
    providedIn: "root"
})
export class SsoProviderService {
    private readonly _ssoLoginError: string;
    private _googleSsoSettings: GoogleSsoSettings;
    private _facebookSsoSettings: FacebookSsoSettings;
    private _linkedinSsoSettings: LinkedInSsoSettings;

    get ssoLoginError(): string {
        return this._ssoLoginError ? unescape(this._ssoLoginError) : null; 
    }
    get googleSsoSettings(): GoogleSsoSettings {
        return this._googleSsoSettings; 
    }
    get facebookSsoSettings(): FacebookSsoSettings {
        return this._facebookSsoSettings; 
    }
    get linkedInSsoSettings(): LinkedInSsoSettings {
        return this._linkedinSsoSettings; 
    }
    get isAnyProviderEnabled(): boolean {
        return this._googleSsoSettings?.enabled ||
            this._facebookSsoSettings?.enabled ||
            this._linkedinSsoSettings?.enabled;
    }

    get googleEndpoint(): string {
        return this._googleSsoSettings && this._googleSsoSettings.enabled
            ? this.document.location.protocol
                + "//" + this.document.location.hostname
                + ":" + this.document.location.port
                + "/SsoProvider/Google"
            : null;
    }

    get facebookEndpoint(): string {
        return this._facebookSsoSettings && this._facebookSsoSettings.enabled
            ? this.document.location.protocol
                + "//" + this.document.location.hostname
                + ":" + this.document.location.port
                + "/SsoProvider/Facebook"
            : null;
    }

    get linkedinEndpoint(): string {
        return this._linkedinSsoSettings && this._linkedinSsoSettings.enabled
            ? this.document.location.protocol
                + "//" + this.document.location.hostname
                + ":" + this.document.location.port
                + "/SsoProvider/LinkedIn"
            : null;
    }


    constructor(@Inject(DOCUMENT) private document: Document,
        private service: SystemConfigurationService) {
        this.initSsoLoginsSetting();
        this._ssoLoginError = CookieService.GetCookie("sso-provider-login-error");
    }


    private async initSsoLoginsSetting() {
        this._googleSsoSettings = this.service.getSettingsSection(SettingsSectionsName.GoogleSsoSettings);
        this._facebookSsoSettings = this.service.getSettingsSection(SettingsSectionsName.FacebookSsoSettings);
        this._linkedinSsoSettings = this.service.getSettingsSection(SettingsSectionsName.LinkedInSsoSettings);
    }
}