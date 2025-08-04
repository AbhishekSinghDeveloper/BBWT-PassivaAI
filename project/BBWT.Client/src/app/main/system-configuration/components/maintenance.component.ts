import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { MessageService } from "primeng/api";
import * as moment from "moment";

import { SystemConfigurationService } from "../system-configuration.service";
import { MaintenanceOptions, MaintenanceSettings } from "../classes/maintenance-settings";
import { SettingsSection } from "../classes/settings-section";
import { SettingsSectionsName } from "../settings-sections-name";
import { Message } from "@bbwt/classes";

@Component({
    selector: "maintenance",
    templateUrl: "maintenance.component.html"
})
export class MaintenanceComponent implements OnInit {
    settings: MaintenanceSettings;

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService,
        private messageService: MessageService) { }

    ngOnInit() {
        this.initSettings();
    }

    save() {
        if (this.areSettingsValid()) {
            this.systemConfigurationService.saveSettings(new SettingsSection(SettingsSectionsName.MaintenanceSettings, this.settings));
        } else {
            this.messageService.add(Message.Error(
                "Please set the correct start and end dates of Maintenance Settings.",
                "System Configuration"
            ));
        }
    }

    private initSettings() {
        this.settings = MaintenanceSettings.parse(this.route.snapshot.data["sysConfig"]);

        if (this.settings.option == null) {
            this.settings.option = 0;
        }

        this.settings.start = !this.settings.start ?
            moment().startOf("day").toDate() :
            moment(this.settings.start).toDate();

        this.settings.end = !this.settings.end ?
            moment().startOf("day").toDate() :
            moment(this.settings.end).toDate();
    }

    private areSettingsValid(): boolean {
        const currentDate = new Date();
        const startDate = this.settings.start;
        const endDate = this.settings.end;
        const isBasic = this.settings.option === MaintenanceOptions.Basic;

        if (!isBasic) return true;

        return startDate > currentDate && startDate < endDate;
    }
}