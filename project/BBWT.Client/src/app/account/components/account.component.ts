import { Component, HostBinding } from "@angular/core";

import { SystemConfigurationService } from "@main/system-configuration";

@Component({
    selector: "account",
    templateUrl: "./account.component.html",
    styleUrls: ["./account.component.scss"]
})
export class AccountComponent {
    @HostBinding("class") hostClass = "account-container";

    constructor(public systemConfigurationService: SystemConfigurationService) {}
}