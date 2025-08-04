import { Component } from "@angular/core";

import { AppStorage } from "@bbwt/utils/app-storage";
import { CoreRole } from "../roles/core-role";
import { SystemConfigTab } from "./system-config-tab";
import { SystemConfigurationService } from "./system-configuration.service";


@Component({
    selector: "system-configuration",
    templateUrl: "./system-configuration.component.html",
    styleUrls: ["./system-configuration.component.scss"]
})
export class SystemConfigurationComponent {
    storedLastOpenedTabName = AppStorage.getItem<string>("system-configuration-last-opened-tab-index");

    systemAdmin: CoreRole = CoreRole.SystemAdmin;
    superAdmin: CoreRole = CoreRole.SuperAdmin;
    tabs = SystemConfigTab;
    pwaEnabled: boolean;


    constructor(private systemConfigurationService: SystemConfigurationService) {
        this.pwaEnabled = systemConfigurationService.pwaEnabled;
    }


    onTabChanged($event: any): void {
        AppStorage.setItem("system-configuration-last-opened-tab-index", $event.originalEvent.currentTarget.textContent);
    }
}