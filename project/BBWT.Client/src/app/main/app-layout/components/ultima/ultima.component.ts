import { Router } from "@angular/router";
import { AfterViewInit, ChangeDetectorRef, Component, HostListener, OnDestroy, OnInit, ViewChild } from "@angular/core";

import { ScrollPanel } from "primeng/scrollpanel";
import { PrimeNGConfig } from "primeng/api";

import { BroadcastService } from "@bbwt/modules/broadcasting";


@Component({
    selector: "ultima-layout",
    templateUrl: "./ultima.component.html"
})
export class UltimaComponent implements OnInit, AfterViewInit, OnDestroy {
    layoutMode = "static";
    darkMenu = false;
    profileMode = "top";
    rotateMenuButton: boolean;
    topbarMenuActive: boolean;
    overlayMenuActive: boolean;
    staticMenuDesktopInactive: boolean;
    staticMenuMobileActive: boolean;
    rightPanelActive: boolean;
    rightPanelClick: boolean;
    menuClick: boolean;
    topbarItemClick: boolean;
    activeTopbarItem: any;
    menuHoverActive: boolean;
    configActive: boolean;
    configClick: boolean;
    inputStyle = "outlined";
    ripple = true;
    compactMode = false;

    @ViewChild("layoutMenuScroller", { static: true }) private layoutMenuScrollerViewChild: ScrollPanel;


    constructor(private cd: ChangeDetectorRef,
                private router: Router,
                private broadcastService: BroadcastService,
                private primengConfig: PrimeNGConfig) {}


    get isTablet(): boolean {
        const width = window.innerWidth;
        return width <= 1024 && width > 640;
    }

    get isDesktop(): boolean {
        return window.innerWidth > 1024;
    }

    get isMobile(): boolean {
        return window.innerWidth <= 640;
    }

    get isOverlay(): boolean {
        return this.layoutMode === "overlay";
    }

    get isStatic(): boolean {
        return this.layoutMode === "static";
    }

    get isHorizontal(): boolean {
        return this.layoutMode === "horizontal";
    }

    get isSlim(): boolean {
        return this.layoutMode === "slim";
    }


    ngOnInit() {
        this.primengConfig.ripple = true;
    }

    ngAfterViewInit(): void {
        setTimeout(() => {
            this.layoutMenuScrollerViewChild.refresh();
        }, 10);
    }

    ngOnDestroy(): void {
        this.layoutMenuScrollerViewChild.refresh();
    }


    @HostListener("window:resize")
    onResize(): void {
        this.cd.detectChanges();
    }

    onLayoutClick() {
        if (!this.topbarItemClick) {
            this.activeTopbarItem = null;
            this.topbarMenuActive = false;
        }

        if (!this.menuClick) {
            if (this.overlayMenuActive || this.staticMenuMobileActive) {
                this.hideOverlayMenu();
            }
        }

        if (!this.rightPanelClick) {
            this.rightPanelActive = false;
        }

        if (this.configActive && !this.configClick) {
            this.configActive = false;
        }

        this.configClick = false;
        this.topbarItemClick = false;
        this.menuClick = false;
        this.rightPanelClick = false;
    }

    onMenuButtonClick(event) {
        this.menuClick = true;
        this.rotateMenuButton = !this.rotateMenuButton;
        this.topbarMenuActive = false;

        if (this.layoutMode === "overlay") {
            this.overlayMenuActive = !this.overlayMenuActive;
        } else {
            if (this.isDesktop) {
                this.staticMenuDesktopInactive = !this.staticMenuDesktopInactive;
            } else {
                this.staticMenuMobileActive = !this.staticMenuMobileActive;
            }
        }

        event.preventDefault();
    }

    onMenuClick($event) {
        this.menuClick = true;
    }

    onTopbarMenuButtonClick(event) {
        this.topbarItemClick = true;
        this.topbarMenuActive = !this.topbarMenuActive;

        this.hideOverlayMenu();

        event.preventDefault();
    }

    onTopbarItemClick(event, item) {
        this.topbarItemClick = true;

        if (this.activeTopbarItem === item) {
            this.activeTopbarItem = null;
        } else {
            this.activeTopbarItem = item;
        }

        event.preventDefault();
    }

    onTopbarSubItemClick(event) {
        event.preventDefault();
    }

    hideOverlayMenu() {
        this.rotateMenuButton = false;
        this.overlayMenuActive = false;
        this.staticMenuMobileActive = false;
    }
}