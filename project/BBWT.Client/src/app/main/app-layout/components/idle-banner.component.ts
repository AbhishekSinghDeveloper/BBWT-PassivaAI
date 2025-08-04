import { Component, OnDestroy, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { DEFAULT_INTERRUPTSOURCES, Idle } from "@ng-idle/core";
import { Subscription } from "rxjs/index";

import { AccountService } from "@account/services";
import { AppStorage } from "@bbwt/utils/app-storage";
import {
    SystemConfigurationService, SettingsSectionsName, SettingsSection, SessionSettings
} from "@main/system-configuration";
import { BroadcastService } from "@bbwt/modules/broadcasting";


@Component({
    selector: "idle-banner",
    templateUrl: "./idle-banner.component.html",
    styleUrls: ["./idle-banner.component.scss"]
})
export class IdleBannerComponent implements OnDestroy {
    private subscription: Subscription;

    showBanner: boolean;
    countDown: number;

    constructor(private idle: Idle,
                private accountService: AccountService,
                private broadcastService: BroadcastService,
                private route: ActivatedRoute) {
        this.configureIdle();
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    private configureIdle(): void {
        if (this.route.snapshot.data["sysConfig"][SettingsSectionsName.UserSessionSettings]) {
            this.refreshIdle(SessionSettings.parse(this.route.snapshot.data["sysConfig"]));
        }

        this.subscription = this.broadcastService.on<SettingsSection>(SystemConfigurationService.SettingsSectionChangedEventName).subscribe(settingsSection => {
            if (settingsSection.sectionName == SettingsSectionsName.UserSessionSettings) {
                this.refreshIdle(settingsSection.value);
            }
        });

        this.idle.onIdleStart.subscribe(() => this.showBanner = true);
        this.idle.onIdleEnd.subscribe(() => this.showBanner = false);
        this.idle.onTimeoutWarning.subscribe((countdown) => this.countDown = countdown);
        this.idle.onTimeout.subscribe(() => {
            this.showBanner = false;
            AppStorage.setItemForSession(AppStorage.LogoutReasonMessageKey, "You've exceeded idle time. Please login again.");
            AppStorage.setItemForSession(AppStorage.LastVisitedPageUrlKey, location.pathname);
            this.accountService.logout();
        });
    }

    private refreshIdle(sessionSettingsSection: SessionSettings): void {
        if (!sessionSettingsSection) return;

        this.idle.clearInterrupts();
        this.idle.stop();
        if (sessionSettingsSection.idleTimeEnabled && sessionSettingsSection.idleTime) {
            this.idle.setIdle((sessionSettingsSection.idleTime - 1) * 60 + 1);
            this.idle.setTimeout(60);
            this.idle.setInterrupts(DEFAULT_INTERRUPTSOURCES);
            this.idle.watch();
        }
    }
}