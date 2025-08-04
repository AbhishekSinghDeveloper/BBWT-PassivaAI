import { Component, OnDestroy, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { MessageService } from "primeng/api";
import { Subscription } from "rxjs/index";
import * as moment from "moment";

import { Message } from "@bbwt/classes";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { UserService } from "../users";
import { SystemConfigurationService, MaintenanceSettings } from "../system-configuration";

@Component({
    selector: "maintenance",
    templateUrl: "./maintenance.component.html",
    styleUrls: ["./maintenance.component.scss"],
})
export class MaintenanceComponent implements OnInit, OnDestroy {
    private showOverlayInterval;
    private hideOverlayInterval;
    private alreadyWarningShown = false;
    private subscription: Subscription;

    overlayIsVisible = false;
    overlayMessage: string;
    overlayRemaining: string;

    constructor(private broadcastService: BroadcastService,
                private userService: UserService,
                private messageService: MessageService,
                private route: ActivatedRoute) { }

    ngOnInit() {
        this.checkMaintenanceSettings(MaintenanceSettings.parse(this.route.snapshot.data["sysConfig"]));

        this.subscription = this.broadcastService.on<MaintenanceSettings>(SystemConfigurationService.MaintenanceSettingsChangedEventName)
            .subscribe(maintenanceSettings => this.checkMaintenanceSettings(maintenanceSettings));
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    private getRemaining(maintenanceSettings: MaintenanceSettings): string {
        return moment(maintenanceSettings.start).format("HH:mm");
    }

    private getDuration(maintenanceSettings: MaintenanceSettings, start?: moment.Moment, withSuffix?: boolean): string {
        return moment.duration(moment(maintenanceSettings.end).diff(start || moment(maintenanceSettings.start), "s"), "s").humanize(withSuffix);
    }

    private checkMaintenanceSettings(maintenanceSettings: MaintenanceSettings) {
        if (!maintenanceSettings || this.userService.isAdmin) {
            return;
        }

        if (!maintenanceSettings.start) {
            console.warn("Maintenance info's start is not defined!");
            return;
        }

        if (!maintenanceSettings.end) {
            console.warn("Maintenance info's end is not definded!");
            return;
        }

        if (this.showOverlayInterval) clearTimeout(this.showOverlayInterval);
        if (this.hideOverlayInterval) clearTimeout(this.hideOverlayInterval);

        const start = moment(maintenanceSettings.start);
        const end = moment(maintenanceSettings.end);
        const now = moment();

        if (now.isBefore(start)) {
            if (start.diff(now, "minutes") < 60) {
                this.showWarning(maintenanceSettings);
            }

            this.showOverlayInterval = setTimeout(() => this.showOverlay(maintenanceSettings), start.diff(now, "milliseconds"));
            this.hideOverlayInterval = setTimeout(() => this.hideOverlay(), end.diff(now, "milliseconds"));
        }

        if (now.isBetween(start, end)) {
            this.showOverlay(maintenanceSettings);

            this.hideOverlayInterval = setTimeout(() => this.hideOverlay(), end.diff(now, "milliseconds"));
        }

        if (now.isAfter(end)) {
            this.hideOverlay();
        }
    }

    private showOverlay(maintenanceSettings: MaintenanceSettings): void {
        this.overlayIsVisible = true;
        this.overlayMessage = maintenanceSettings.message;
        this.overlayRemaining = this.getDuration(maintenanceSettings, moment(), true);
    }

    private hideOverlay(): void {
        this.overlayIsVisible = false;
    }

    private showWarning(maintenanceSettings: MaintenanceSettings) {
        if (this.alreadyWarningShown) return;

        this.alreadyWarningShown = true;
        const remaining = this.getRemaining(maintenanceSettings);
        const duration = this.getDuration(maintenanceSettings);

        this.messageService.add(Message.Warning(`The site will be shut down after ${remaining} and unavailable for ${duration}`, "Scheduled maintenance"));
    }
}