import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Route, Router, RouterStateSnapshot, UrlSegment } from "@angular/router";

import { MessageService } from "primeng/api";

import { UserService } from "@main/users";
import { SecurityService } from "./security.service";
import { Message } from "../../classes";
import { constructPathFromRoute } from "../../utils/routes";


@Injectable()
export class SecurityGuard  {
    private readonly _loginUrl = "/account/login";
    private readonly _homeUrl = "/app";
    private readonly _twoFactorAuthenticationUrl = "/app/profile/authentication";

    constructor(private router: Router,
                private messageService: MessageService,
                private userService: UserService,
                private securityService: SecurityService) {
    }

    async canLoad(route: Route, segments: UrlSegment[]): Promise<boolean> {
        if (this.userService.isLogged) {
            return true;
        } else {         
            // Originally the redirectStr was missing the query parameters, segments didn't have them
            // So I have to extract them in another way and add them at the end so the redirectStr will work for FormIO
            const params = this.router.getCurrentNavigation().extractedUrl.queryParams;
            const keys = Object.keys(params);
            const query = `?${keys.map(x => `${x}=${params[x]}`).join("&")}`;
            const base = segments?.map(x => `/${x.path}`).join("");
            await this.router.navigate(
                [this._loginUrl],
                {
                    queryParams: {
                        redirectStr: (!base) ? "" : base + query
                    }
                });
            return false;
        }
    }

    async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        await this.securityService.refreshRoutes(false);

        if (this.userService.isLogged &&
            this.userService.isUserRequiredSetupTwoFactor &&
            state.url != this._twoFactorAuthenticationUrl
        ) {
            await this.router.navigate([this._twoFactorAuthenticationUrl]);
            return false;
        }
        if (!this.userService.isLogged) {
            await this.router.navigate([this._loginUrl]);
            return false;
        }

        if (Object.keys(route.children).length) {
            return true;
        }

        const path = [state.url];
        path.push(constructPathFromRoute(route));

        if (this.securityService.isPageAvailable(path)) {
            return true;
        } else {
            this.messageService.add(Message.Error("Access denied."));
            await this.router.navigate([this._homeUrl]);
            return false;
        }
    }

    canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        return this.canActivate(childRoute, state);
    }
}