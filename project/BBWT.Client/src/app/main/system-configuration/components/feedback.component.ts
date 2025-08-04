import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { MessageService } from "primeng/api";

import { SystemConfigurationService } from "../system-configuration.service";
import { FeedbackSettings } from "../classes/feedback-settings";
import { SettingsSection } from "../classes/settings-section";
import { SettingsSectionsName } from "../settings-sections-name";
import { isLocalhost } from "@bbwt/utils/location";
import { Message } from "@bbwt/classes";

@Component({
    selector: "feedback",
    templateUrl: "feedback.component.html"
})
export class FeedbackComponent  implements OnInit {
    settings: FeedbackSettings;
    isLocalhost = isLocalhost();

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService,
        private messageService: MessageService) { }

    ngOnInit() {
        this.settings = FeedbackSettings.parseSection(this.route.snapshot.data["sysConfig"]);
    }

    save() {
        this.systemConfigurationService.saveSettings(new SettingsSection(SettingsSectionsName.FeedbackSettings, this.settings))
            .then(() => this.messageService.add(Message.Info("These changes requires pressing F5 to see the changes applied.", "System Configuration")));
    }
}