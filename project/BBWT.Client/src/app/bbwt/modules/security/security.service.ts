import { Injectable } from "@angular/core";

import { RoutesService } from "@main/routes";
import { AdminService } from "@main/admin/services/admin.service";
import { CurrentUserChangedData, UserService } from "@main/users";
import { BroadcastService } from "../broadcasting";
import { AppStorage } from "../../utils/app-storage";
import { MainMenuService } from "@main/menu-designer";
import { StaticPageService } from "@main/static-pages";


@Injectable({
    providedIn: "root"
})
export class SecurityService {
    private readonly allowedPageRoutesKey = "allowed-page-routes";

    private allowedPagesPaths: string[] = [];


    constructor(private routesService: RoutesService,
                private adminService: AdminService,
                private broadcastService: BroadcastService) {
        this.allowedPagesPaths = <string[]>AppStorage.getItem(this.allowedPageRoutesKey);

        broadcastService.on<CurrentUserChangedData>(UserService.CurrentUserChangedEventName).subscribe(data => {
            if (data.isLogout) {
                this.allowedPagesPaths = null;
            } else {
                if (data.identityChanged || data.accessRightsChanged) {
                    this.refreshRoutes(true);
                }
            }
        });

        //TODO: why should security service "know" about main menu & static pages features?
        // It's seems the last ones should know about security service instead and force routes refreshing,
        // Because only main menu and static pages in their logic "know" whether their changes lead to routes changes or not.
        broadcastService.on(MainMenuService.MainMenuChangedEventName).subscribe(() => this.refreshRoutes(true));
        broadcastService.on(StaticPageService.StaticPagesChangedEventName).subscribe(() => this.refreshRoutes(true));
    }


    isPageAvailable(path: string[]): boolean {
        if (!this.allowedPagesPaths || !path || path.length == 0) {
            return false;
        }

        if (path.some(p => p.startsWith("/app/admin")) && !this.adminService.accessible) {
            return false;
        }

        return this.allowedPagesPaths.find(allowedPagesPathItem => path.some(pathItem => pathItem === allowedPagesPathItem)) != null;
    }

    refreshRoutes(force = true): Promise<string[]> {
        if (force || !this.allowedPagesPaths) {
            return this.routesService.getCurrentUserAllowedRoutesPaths().then(allowedPagesPaths => {
                this.allowedPagesPaths = allowedPagesPaths;
                AppStorage.setItem(this.allowedPageRoutesKey, allowedPagesPaths);
                return allowedPagesPaths;
            });
        } else {
            return Promise.resolve(this.allowedPagesPaths);
        }
    }
}