import { Injectable } from "@angular/core";
import { fromEvent, Subscription } from "rxjs";

import { MessageService } from "primeng/api";

import { Message } from "@bbwt/classes";
import { BroadcastService } from "@bbwt/modules/broadcasting";


@Injectable()
export class AppOnlineStateService {
    static readonly AppOnlineStateChanged = "AppOnlineStateChanged";

    isAppOnline = true;

    private onlineEventSubscription: Subscription;
    private offlineEventSubscription: Subscription;

    constructor(
        private messageService: MessageService,
        private broadcastService: BroadcastService
    ) {
        if (navigator.onLine) return;

        this.isAppOnline = false;
        setTimeout(this.showAppOfflineWarning, 3000);
    }

    subscribeAppStateEvents() {
        this.subscribeOnlineEvent();
        this.subscribeOfflineEvent();
    }

    unsubscribeAppStateEvents() {
        this.onlineEventSubscription?.unsubscribe();
        this.offlineEventSubscription?.unsubscribe();
    }

    private subscribeOnlineEvent() {
        this.onlineEventSubscription = fromEvent(window, "online")
            .subscribe(() => {
                this.isAppOnline = true;
                this.broadcastService.broadcast(AppOnlineStateService.AppOnlineStateChanged, { isAppOnline: true });
            });
    }

    private subscribeOfflineEvent() {
        this.offlineEventSubscription = fromEvent(window, "offline").subscribe(() => {
            this.isAppOnline = false;
            this.showAppOfflineWarning();
            this.broadcastService.broadcast(AppOnlineStateService.AppOnlineStateChanged, { isAppOnline: false });
        });
    }

    private showAppOfflineWarning() {
        this.messageService.add(Message.Warning("Application is in offline mode"));
    }
}