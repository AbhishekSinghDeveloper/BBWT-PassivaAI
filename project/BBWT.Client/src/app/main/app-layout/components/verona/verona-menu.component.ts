import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from "@angular/core";

import { VeronaComponent } from "./verona.component";
import { ScrollPanel } from "primeng/scrollpanel";
import { AppComponent } from "../app.component";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { Subscription } from "rxjs";
import { AppStorage } from "@bbwt/utils/app-storage";
import { LayoutEventKeys } from "@main/app-layout/layout-event-keys";
import { LayoutDataKeys } from "../../layout-data-keys";

@Component({
    selector: "verona-menu",
    templateUrl: "./verona-menu.component.html",
    styleUrls: ["./verona-menu.component.scss"]
})
export class VeronaMenuComponent implements AfterViewInit, OnDestroy, OnInit {
    isMenuBlocked: boolean;

    private _subscription: Subscription;

    @ViewChild("layoutMenuScroller", { static: true }) private layoutMenuScrollerViewChild: ScrollPanel;

    constructor(public app: AppComponent, private broadcastService: BroadcastService, public veronaMain: VeronaComponent) {}

    ngOnInit(): void {
        this.hideMenuIfSmallWindow();

        this.isMenuBlocked =  AppStorage.getItem<boolean>(LayoutDataKeys.IsMenuBlockedKey);
        this._subscription = this.broadcastService.on<boolean>(LayoutEventKeys.BlockMenuEventName).subscribe(isMenuBlocked => {
            this.isMenuBlocked = isMenuBlocked;
        });
    }

    ngAfterViewInit(): void {
        setTimeout(() => {
            this.layoutMenuScrollerViewChild?.refresh();
        }, 10);
    }

    ngOnDestroy(): void {
        this.layoutMenuScrollerViewChild?.refresh();
        this._subscription?.unsubscribe();
    }

    checkSize(event: any) {
        this.hideMenuIfSmallWindow();
    }

    // A solution to auto-hide menu panel specific for Verona template
    private hideMenuIfSmallWindow() {
        this.veronaMain.menuActive = !this.veronaMain.isTablet && !this.veronaMain.isMobile;
    }
}
