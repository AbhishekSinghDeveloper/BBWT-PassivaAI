import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { MessageService } from "primeng/api";

import { SystemConfigurationService } from "../system-configuration.service";
import { SettingsSection } from "../classes/settings-section";
import { SessionSettings } from "../classes/session-settings";
import { SettingsSectionsName } from "../settings-sections-name";
import { Message } from "@bbwt/classes";

@Component({
    selector: "session",
    templateUrl: "session.component.html"
})
export class SessionComponent implements OnInit {
    sessionSettings: SessionSettings;

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService,
        private messageService: MessageService) { }

    ngOnInit() {
        this.initSettings();
    }

    save() {
        this.systemConfigurationService.saveSettings(
            new SettingsSection(SettingsSectionsName.UserSessionSettings, this.sessionSettings)
        ).then(() => this.messageService.add(Message.Info("These changes requires pressing F5 to see the changes applied.", "System Configuration")));
    }

    private initSettings() {
        const config = this.route.snapshot.data["sysConfig"];

        this.sessionSettings = SessionSettings.parse(config);
    }
}