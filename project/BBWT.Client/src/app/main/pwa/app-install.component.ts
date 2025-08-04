import { Component } from "@angular/core";

import { PwaService } from "./pwa.service";


@Component({
    selector: "app-install",
    templateUrl: "./app-install.component.html",
})
export class AppInstallComponent {
    constructor(public pwaService: PwaService) {}


    installApp(): void {
        this.pwaService.installationPromptEvent.prompt();
    }

    askLater(): void {
        this.pwaService.declinePrompt();
    }

    onHide(): void {
        if (this.pwaService.isApplePwaCompatibleDevice || this.pwaService.appInstallationAvailable) {
            this.pwaService.declinePrompt();
        }
    }
}