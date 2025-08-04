import { Component, OnInit } from "@angular/core";
import { SwUpdate } from "@angular/service-worker";

@Component({
    selector: "app-auto-update",
    templateUrl: "./app-auto-update.component.html",
})
export class AppAutoUpdateComponent implements OnInit {
    autoUpdateReloadSeconds = 30;
    isUpdateAvailable = false;

    private intervalRef: NodeJS.Timeout;


    constructor(private swUpdate: SwUpdate) { }


    ngOnInit(): void {
        if (!this.swUpdate.isEnabled) return;

        this.swUpdate.versionUpdates
            .subscribe(() => {
                this.showUpdateAvailableWarning();
                this.startAutoReloadCounter();
            });
    }

    onAppReload(): void {
        this.activateUpdate();
    }


    private showUpdateAvailableWarning(): void {
        this.isUpdateAvailable = true;
    }

    private startAutoReloadCounter(): void {
        this.intervalRef = setInterval(() => {
            if (--this.autoUpdateReloadSeconds != 0) return;
            this.activateUpdate();
        }, 1000);
    }

    private activateUpdate(): void {
        this.resetCounter();
        this.swUpdate
            .activateUpdate()
            .then(() => window.location.reload())
            .catch(err => console.error(err));
    }

    private resetCounter(): void {
        clearInterval(this.intervalRef);
        this.isUpdateAvailable = false;
    }
}
