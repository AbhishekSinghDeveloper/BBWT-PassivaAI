import { Component, OnInit } from "@angular/core";

import { PrimeNGConfig } from "primeng/api";

import { MainMenuService } from "@main/menu-designer";


@Component({
    selector: "verona-layout",
    templateUrl: "./verona.component.html"
})
export class VeronaComponent implements OnInit {
    menuMode = "static";
    menuActive = true;
    topbarMenuActive = false;
    activeTopbarItem: Element;
    menuClick: boolean;
    menuButtonClick: boolean;
    topbarMenuButtonClick: boolean;
    menuHoverActive: boolean;
    inputStyle = "outlined";
    ripple: boolean;
    configActive: boolean;
    configClick: boolean;


    constructor(private menuService: MainMenuService, private primengConfig: PrimeNGConfig) {}


    get isMobile(): boolean {
        return window.innerWidth < 1025;
    }

    get isHorizontal(): boolean {
        return this.menuMode === "horizontal";
    }

    get isTablet(): boolean {
        const width = window.innerWidth;
        return width <= 1024 && width > 640;
    }


    ngOnInit() {
        this.primengConfig.ripple = true;
    }


    onMenuButtonClick(event: Event) {
        this.menuButtonClick = true;
        this.menuActive = !this.menuActive;
        event.preventDefault();
    }

    onTopbarMenuButtonClick(event: Event) {
        this.topbarMenuButtonClick = true;
        this.topbarMenuActive = !this.topbarMenuActive;
        event.preventDefault();
    }

    onTopbarItemClick(event: Event, item: Element) {
        this.topbarMenuButtonClick = true;

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

    onLayoutClick() {
        if (!this.menuButtonClick && !this.menuClick) {
            if (this.menuMode === "horizontal") {
                this.menuService.reset();
            }

            if (this.isMobile || this.menuMode === "overlay" || this.menuMode === "popup") {
                this.menuActive = false;
            }

            this.menuHoverActive = false;
        }

        if (!this.topbarMenuButtonClick) {
            this.activeTopbarItem = null;
            this.topbarMenuActive = false;
        }

        if (this.configActive && !this.configClick) {
            this.configActive = false;
        }

        this.configClick = false;
        this.menuButtonClick = false;
        this.menuClick = false;
        this.topbarMenuButtonClick = false;
    }

    onRippleChange(event) {
        this.ripple = event.checked;
    }

    onConfigClick(event) {
        this.configClick = true;
    }

    onMenuClick() {
        this.menuClick = true;
    }
}
