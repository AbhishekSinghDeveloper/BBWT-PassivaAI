import { Component, OnInit } from "@angular/core";
import { AppOnlineStateService } from "./app-online-state.service";
import { BroadcastService } from "@bbwt/modules/broadcasting";

@Component({
    selector: "app-offline-indicator",
    templateUrl: "./app-offline-indicator.component.html"
})
export class AppOfflineIndicatorComponent implements OnInit {
    isOffline = false;

    constructor(
        private appOnlineStateService: AppOnlineStateService,
        private broadcastService: BroadcastService
    ) { }

    ngOnInit() {
        this.isOffline = !this.appOnlineStateService.isAppOnline;

        this.broadcastService.on<{ isAppOnline: boolean }>(AppOnlineStateService.AppOnlineStateChanged)
            .subscribe(data => this.isOffline = !data.isAppOnline);
    }
}
