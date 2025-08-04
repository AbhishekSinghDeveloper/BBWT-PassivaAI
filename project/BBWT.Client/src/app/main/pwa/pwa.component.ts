import { Component, OnInit, ViewEncapsulation } from "@angular/core";

import { AppOnlineStateService } from "./app-online-state.service";
import { SystemConfigurationService } from "@main/system-configuration";


@Component({
    selector: "pwa",
    templateUrl: "./pwa.component.html",
    styleUrls: ["./pwa.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class PwaComponent implements OnInit {
    serviceWorkerRegistered = false;

    constructor(private systemConfigurationService: SystemConfigurationService, private appOnlineStateService: AppOnlineStateService) {}

    ngOnInit(): void {
        this.appOnlineStateService.subscribeAppStateEvents();

        if (!navigator["serviceWorker"] || !this.systemConfigurationService.pwaEnabled) return;

        navigator.serviceWorker.ready.then(() => this.serviceWorkerRegistered = true);
    }
}