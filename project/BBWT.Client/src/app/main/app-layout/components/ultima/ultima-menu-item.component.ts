import { Component, Input, OnInit, ChangeDetectorRef, OnDestroy, HostBinding } from "@angular/core";
import { Router, NavigationEnd } from "@angular/router";
import { trigger, state, style, transition, animate } from "@angular/animations";

import { Subscription } from "rxjs";
import { filter } from "rxjs/operators";

import { UltimaComponent } from "./ultima.component";
import { IMainMenuItem, MainMenuService } from "@main/menu-designer";
import { ProjectSettings, SystemConfigurationService } from "@main/system-configuration";
import { DefaultProjectThemes } from "../../../system-configuration/classes/project-settings-themes";

@Component({
    selector: "[ultima-menu-item]",
    templateUrl: "./ultima-menu-item.component.html",
    animations: [
        trigger("children", [
            state(
                "void",
                style({
                    height: "0px"
                })
            ),
            state(
                "hiddenAnimated",
                style({
                    height: "0px"
                })
            ),
            state(
                "visibleAnimated",
                style({
                    height: "*"
                })
            ),
            state(
                "visible",
                style({
                    height: "*",
                    "z-index": 100
                })
            ),
            state(
                "hidden",
                style({
                    height: "0px",
                    "z-index": "*"
                })
            ),
            transition("visibleAnimated => hiddenAnimated", animate("400ms cubic-bezier(0.86, 0, 0.07, 1)")),
            transition("hiddenAnimated => visibleAnimated", animate("400ms cubic-bezier(0.86, 0, 0.07, 1)")),
            transition(
                "void => visibleAnimated, visibleAnimated => void",
                animate("400ms cubic-bezier(0.86, 0, 0.07, 1)")
            )
        ])
    ]
})
export class UltimaMenuItemComponent implements OnInit, OnDestroy {
    @Input() item: IMainMenuItem;
    @Input() index: number;
    @Input() root: boolean;
    @Input() parentKey: string;

    @HostBinding("class.active-menuitem")
    active = false;
    key: string;

    private readonly menuSourceSubscription: Subscription;
    private readonly menuResetSubscription: Subscription;

    constructor(
        public ultimaMain: UltimaComponent,
        private router: Router,
        private cd: ChangeDetectorRef,
        private menuService: MainMenuService,
        private systemConfigurationService: SystemConfigurationService
    ) {
        this.menuSourceSubscription = this.menuService.menuSource$.subscribe(key => {
            // Deactivate current active menu
            if (this.active && this.key !== key && key.indexOf(this.key) !== 0) {
                this.active = false;
            }
        });

        this.menuResetSubscription = this.menuService.resetSource$.subscribe(() => {
            this.active = false;
        });

        this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(params => {
            if (this.ultimaMain.isHorizontal || this.ultimaMain.isSlim) {
                this.active = false;
            } else {
                this.updateActiveStateFromRoute();
            }
        });
    }

    ngOnInit() {
        if (!this.ultimaMain.isHorizontal || !this.ultimaMain.isSlim) {
            this.updateActiveStateFromRoute();
        }

        this.key = this.parentKey ? this.parentKey + "-" + this.index : String(this.index);
    }

    ngOnDestroy() {
        if (this.menuSourceSubscription) {
            this.menuSourceSubscription.unsubscribe();
        }

        if (this.menuResetSubscription) {
            this.menuResetSubscription.unsubscribe();
        }
    }

    updateActiveStateFromRoute(): void {
        if (this.item.routerLink == "/app") {
            this.active = location.pathname == "/app";
        } else {
            this.active = this.isItemActive(this.item);
        }
    }

    isItemActive(item: IMainMenuItem): boolean {
        let result = false;

        if (item.children?.length) {
            item.children.forEach(x => (result = result || this.isItemActive(x)));
        } else {
            if (item.routerLink) {
                /** https://angular.io/api/router/Router#isactive */
                result =
                    result ||
                    this.router.isActive(item.routerLink, {
                        paths: "subset",
                        queryParams: "subset",
                        fragment: "ignored",
                        matrixParams: "ignored"
                    });
            }
        }

        return result;
    }

    shouldShowRouterItem(): boolean {
        return !!this.item.routerLink;
    }

    shouldShowHrefItem(): boolean {
        return this.item.href && !this.item.routerLink;
    }

    shouldShowLabelItem(): boolean {
        return !this.item.href && !this.item.routerLink && !this.item.customHandler && !!this.item.children?.length;
    }

    shouldShowCustomHandledItem(): boolean {
        return !!this.item.customHandler;
    }

    onMouseEnter() {
        // Activate item on hover
        if (
            this.root &&
            this.ultimaMain.menuHoverActive &&
            (this.ultimaMain.isHorizontal || this.ultimaMain.isSlim) &&
            this.ultimaMain.isDesktop
        ) {
            this.menuService.onMenuStateChange(this.key);
            this.active = true;
        }
    }

    itemClick(event: Event) {
        if (!!this.item.routerLink || this.item.href) {
            return true;
        }

        event.preventDefault();

        // Avoid processing disabled items
        if (this.item.disabled) {
            return true;
        }

        // Navigate with hover in horizontal mode
        if (this.root) {
            this.ultimaMain.menuHoverActive = !this.ultimaMain.menuHoverActive;
        }

        // Notify other items
        this.menuService.onMenuStateChange(this.key);

        // Execute command
        if (this.item.customHandler) {
            this.applyCustomHandler(this.item);
        }

        // Toggle active state
        if (this.item.children) {
            this.active = !this.active;
        } else {
            // Activate item
            this.active = true;

            // Reset horizontal menu
            if (this.ultimaMain.isHorizontal || this.ultimaMain.isSlim) {
                this.menuService.reset();
            }

            // Hide overlay menus
            this.ultimaMain.overlayMenuActive = false;
            this.ultimaMain.staticMenuMobileActive = false;
            this.ultimaMain.menuHoverActive = !this.ultimaMain.menuHoverActive;
        }
    }

    private applyCustomHandler(item: IMainMenuItem): void {
        if (!item.customHandler) {
            throw new Error("This item does not assumes custom handling.");
        }

        switch (item.customHandler) {
            case "theme":
                const theme = DefaultProjectThemes.GetThemeByName(item.label);
                if (theme) {
                    this.systemConfigurationService.setThemeCode(theme.code);
                } else {
                    throw new Error(`The theme with label "${item.label}" not found.`);
                }
                break;
        }
    }
}
