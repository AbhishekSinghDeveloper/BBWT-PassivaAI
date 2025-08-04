import { Component, OnDestroy, OnInit } from "@angular/core";

import { firstValueFrom, Subscription } from "rxjs";
import * as moment from "moment";

import { BroadcastService } from "@bbwt/modules/broadcasting";
import {
    MaintenanceOptions,
    MaintenanceSettings,
    SettingsSection,
    SettingsSectionsName
} from "../system-configuration";
import { SystemConfigurationService } from "../system-configuration";
import { ActivatedRoute } from "@angular/router";
import { HttpClient } from "@angular/common/http";

@Component({
    selector: "maintenance-banner",
    templateUrl: "./maintenance-banner.component.html",
    styleUrls: ["./maintenance-banner.component.scss"]
})
export class MaintenanceBannerComponent implements OnDestroy {
    private showBannerInterval;
    private hideBannerInterval;
    private subscription: Subscription;

    bannerVisible: boolean;
    collapsed: boolean;
    timeStart: string;
    timeEnd: string;
    message: string;

    constructor(private broadcastService: BroadcastService, private route: ActivatedRoute, private http: HttpClient) {
        if (this.route.snapshot.data["sysConfig"][SettingsSectionsName.MaintenanceSettings]) {
            this.maintenanceSettingsChanged(MaintenanceSettings.parse(this.route.snapshot.data["sysConfig"]));
        }

        this.subscription = broadcastService
            .on<MaintenanceSettings>(SystemConfigurationService.MaintenanceSettingsChangedEventName)
            .subscribe(maintenanceSettings => this.maintenanceSettingsChanged(maintenanceSettings));
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    private async maintenanceSettingsChanged(maintenanceSettings: MaintenanceSettings): Promise<void> {
        if (!maintenanceSettings || !maintenanceSettings.isActive) return;

        if (maintenanceSettings.option === MaintenanceOptions.External && maintenanceSettings.externalApiUrl) {
            const request = this.http.get<MaintenanceSettings>(maintenanceSettings.externalApiUrl);
            const externalSettings = await firstValueFrom(request);

            if (!externalSettings || !externalSettings.isActive) {
                return;
            } else {
                maintenanceSettings = externalSettings;
            }
        }

        if (this.showBannerInterval) clearTimeout(this.showBannerInterval);
        if (this.hideBannerInterval) clearTimeout(this.hideBannerInterval);

        const start = moment(maintenanceSettings.start);
        const end = moment(maintenanceSettings.end);
        const now = moment();

        if (now.isBefore(start)) {
            this.showBannerInterval = setTimeout(
                () => this.showBanner(maintenanceSettings),
                start.diff(now, "milliseconds")
            );
            this.hideBannerInterval = setTimeout(() => this.hideBanner(), end.diff(now, "milliseconds"));
        }

        if (now.isBetween(start, end)) {
            this.showBanner(maintenanceSettings);

            this.hideBannerInterval = setTimeout(() => this.hideBanner(), end.diff(now, "milliseconds"));
        }

        if (now.isAfter(end)) {
            this.hideBanner();
        }
    }

    private showBanner(maintenanceSettings: MaintenanceSettings): void {
        if (maintenanceSettings) {
            this.message = maintenanceSettings.message;
            this.timeStart = moment(maintenanceSettings.start).format("HH:mm");
            this.timeEnd = moment(maintenanceSettings.end).format("HH:mm");
        }

        this.bannerVisible = true;
    }

    private hideBanner(): void {
        this.bannerVisible = false;
    }
}
