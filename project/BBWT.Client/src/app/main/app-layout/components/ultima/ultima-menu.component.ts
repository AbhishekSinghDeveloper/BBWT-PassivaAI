import { Component, OnDestroy, OnInit } from "@angular/core";

import { BroadcastService } from "@bbwt/modules/broadcasting";
import { AppComponent } from "../app.component";
import { LayoutEventKeys } from "@main/app-layout/layout-event-keys";
import { LayoutDataKeys } from "@main/app-layout/layout-data-keys";
import { Subscription } from "rxjs";
import { AppStorage } from "@bbwt/utils/app-storage";



@Component({
    selector: "ultima-menu",
    templateUrl: "./ultima-menu.component.html",
    styleUrls: ["./ultima-menu.component.scss"]
})
export class UltimaMenuComponent implements OnInit, OnDestroy {
    isMenuBlocked: boolean;

    private _subscription: Subscription;

    constructor(public app: AppComponent, private broadcastService: BroadcastService) {}

    ngOnInit(): void {
        this.isMenuBlocked = this.isMenuBlocked = AppStorage.getItem<boolean>(LayoutDataKeys.IsMenuBlockedKey);
        this._subscription = this.broadcastService.on<boolean>(LayoutEventKeys.BlockMenuEventName).subscribe(isMenuBlocked => {
            this.isMenuBlocked = this.isMenuBlocked = isMenuBlocked;
        });
    }

    ngOnDestroy(): void {
        this._subscription?.unsubscribe();
    }
}
