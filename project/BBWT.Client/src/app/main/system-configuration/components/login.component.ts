import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Message } from "@bbwt/classes/message";
import { MessageService } from "primeng/api";
import { FacebookSsoSettings, FailedLoginAttemptsPolicySettings, GoogleSsoSettings, LinkedInSsoSettings } from "..";
import { LoginSettings } from "../classes/login-settings";
import { ReCaptchaSettings } from "../classes/recaptcha-settings";
import { SettingsSection } from "../classes/settings-section";
import { TwoFactorMandatoryMode, TwoFactorSettings } from "../classes/two-factor-settings";
import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationService } from "../system-configuration.service";

@Component({
    selector: "login",
    templateUrl: "login.component.html",
    styleUrls: ["login.component.scss"],
})
export class LoginComponent implements OnInit {
    loginSettings: LoginSettings;
    reCaptchaSettings: ReCaptchaSettings;
    googleSsoSettings: GoogleSsoSettings;
    facebookSsoSettings: FacebookSsoSettings;
    linkedInSsoSettings: LinkedInSsoSettings;
    twoFactorSettings: TwoFactorSettings;
    failedLoginAttemptsPolicySettings: FailedLoginAttemptsPolicySettings;

    TwoFactorModeEnum = TwoFactorMandatoryMode;

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService,
        private messageService: MessageService
    ) {}

    ngOnInit() {
        const data = this.route.snapshot.data["sysConfig"];

        try {
            this.googleSsoSettings = GoogleSsoSettings.parse(data);
        } catch (err) {
            console.error(err);
        }

        try {
            this.facebookSsoSettings = FacebookSsoSettings.parse(data);
        } catch (err) {
            console.error(err);
        }

        try {
            this.linkedInSsoSettings = LinkedInSsoSettings.parse(data);
        } catch (err) {
            console.error(err);
        }

        try {
            this.reCaptchaSettings = ReCaptchaSettings.parse(data);
        } catch (err) {
            console.error(err);
        }

        try {
            this.twoFactorSettings = TwoFactorSettings.parse(data);

            if (this.twoFactorSettings.mandatoryMode == null) {
                this.twoFactorSettings.mandatoryMode = TwoFactorMandatoryMode.Optional;
            }
        } catch (err) {
            console.error(err);
        }

        try {
            this.failedLoginAttemptsPolicySettings = FailedLoginAttemptsPolicySettings.parse(data);
        } catch (err) {
            console.error(err);
        }
    }

    save(): void {
        this.systemConfigurationService
            .saveSettings(
                new SettingsSection(SettingsSectionsName.GoogleSsoSettings, this.googleSsoSettings),
                new SettingsSection(
                    SettingsSectionsName.FacebookSsoSettings,
                    this.facebookSsoSettings
                ),
                new SettingsSection(
                    SettingsSectionsName.LinkedInSsoSettings,
                    this.linkedInSsoSettings
                ),
                new SettingsSection(SettingsSectionsName.ReCaptchaSettings, this.reCaptchaSettings),
                new SettingsSection(SettingsSectionsName.TwoFactorSettings, this.twoFactorSettings),
                new SettingsSection(
                    SettingsSectionsName.FailedAttemptsPassword,
                    this.failedLoginAttemptsPolicySettings
                )
            )
            .then(() => {
                this.messageService.add(
                    Message.Warning(
                        "New SSO settings will be applied after restarting the application"
                    )
                );
            });
    }

    onGoogleSsoEnabledChanged(): void {
        if (!this.googleSsoSettings.enabled) {
            this.googleSsoSettings.clientId = "";
            this.googleSsoSettings.clientSecret = "";
        }
    }

    onFacebookSsoEnabledChanged(): void {
        if (!this.facebookSsoSettings.enabled) {
            this.facebookSsoSettings.appId = "";
            this.facebookSsoSettings.appSecret = "";
        }
    }

    onLinkedInSsoEnabledChanged(): void {
        if (!this.linkedInSsoSettings.enabled) {
            this.linkedInSsoSettings.clientId = "";
            this.linkedInSsoSettings.clientSecret = "";
        }
    }
}
