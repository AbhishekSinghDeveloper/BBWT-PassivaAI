import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { SettingsSection } from "../classes/settings-section";
import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationService } from "../system-configuration.service";
import { LoadingTimeSettings } from "../../loading-time/loading-time-settings";

@Component({
    selector: "performance",
    templateUrl: "performance.component.html"
})
export class PerformanceComponent implements OnInit {
    settings: LoadingTimeSettings;

    constructor(
        private systemConfigurationService: SystemConfigurationService,
        private route: ActivatedRoute) { }

    ngOnInit() {
        this.settings = LoadingTimeSettings.parse(this.route.snapshot.data["sysConfig"]);
    }

    save() {
        this.systemConfigurationService.saveSettings(new SettingsSection(SettingsSectionsName.LoadingTimeSettings, this.settings));
    }
}