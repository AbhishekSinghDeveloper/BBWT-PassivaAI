import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { SelectItem } from "primeng/api";

import { SystemConfigurationService } from "../system-configuration.service";
import { RegistrationSettings } from "../classes/registration-settings";
import { SettingsSection } from "../classes/settings-section";
import { SettingsSectionsName } from "../settings-sections-name";
import { OrganizationService } from "../../organizations";

@Component({
    selector: "registration",
    templateUrl: "registration.component.html"
})
export class RegistrationComponent implements OnInit {
    settings: RegistrationSettings;
    organizationsOptions: SelectItem[];

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService,
        private companyService: OrganizationService) { }

    ngOnInit() {
        this.settings = RegistrationSettings.parse(this.route.snapshot.data["sysConfig"]);
        this.initOrganizationsOptions();
    }

    save() {
        this.systemConfigurationService.saveSettings(new SettingsSection(SettingsSectionsName.RegistrationSettings, this.settings));
    }

    private initOrganizationsOptions() {
        this.companyService.getAllPlain().then(organizations => {
            this.organizationsOptions = organizations.map(companyItem => <SelectItem>{ label: companyItem.name, value: companyItem.id_original });
        });
    }
}