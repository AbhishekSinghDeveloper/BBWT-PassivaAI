import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { SystemConfigurationService } from "../system-configuration.service";
import { UserPasswordSettings } from "../classes/user-password-settings";
import { SettingsSection } from "../classes/settings-section";
import { SettingsSectionsName } from "../settings-sections-name";

@Component({
    selector: "password",
    templateUrl: "password.component.html"
})
export class PasswordComponent implements OnInit {
    settings: UserPasswordSettings;

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService) { }

    ngOnInit() {
        this.settings = UserPasswordSettings.parse(this.route.snapshot.data["sysConfig"]);

        if (this.settings.passwordReuse == null) {
            this.settings.passwordReuse = 0;
        }
    }

    save(): void {
        this.systemConfigurationService.saveSettings(new SettingsSection(SettingsSectionsName.UserPasswordSettings, this.settings));
    }
}