import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { SystemConfigurationService } from "../system-configuration.service";
import { PwaSettings } from "../classes/pwa-settings";
import { SettingsSection } from "../classes/settings-section";
import { SettingsSectionsName } from "../settings-sections-name";


@Component({
    selector: "pwa-settings",
    templateUrl: "./pwa-settings.component.html"
})
export class PwaSettingsComponent implements OnInit {
    settings: PwaSettings;

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService) { }

    ngOnInit() {
        this.settings = PwaSettings.parse(this.route.snapshot.data["sysConfig"]);
    }

    save(): void {
        this.systemConfigurationService.saveSettings(new SettingsSection(SettingsSectionsName.PwaSettings, this.settings));
    }
}