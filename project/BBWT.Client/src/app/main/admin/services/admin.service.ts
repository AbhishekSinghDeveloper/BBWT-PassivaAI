import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { UserService } from "../../users";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { IUser } from "../../users";
import { AccountService } from "@account/services/account.service";
import { AppStorage } from "@bbwt/utils/app-storage";


@Injectable()
export class AdminService {
    private _accessible: boolean;


    constructor(private httpClient: HttpClient,
                private accountService: AccountService,
                private userService: UserService,
        private broadcastService: BroadcastService) {
        // Logic of refreshCurrentUserAdminState() looks incomplete/inconsistent.
        // The method requests JWT token so then to request /check-auth to a separate instance of Web.Admin app
        // In order to access/block the admin UI and that would mean if we set up JWT response for some other purpose
        // Then the main app's admin UI wouldn't work without separate instance of Web.Admin app, that's odd.
        // This code is commented until we discuss/redesign the purpose of Web.Admin app.
        // BroadcastService.on<IUser>(UserService.CurrentUserChangedEventName)
        //    .subscribe(currentUser => this.refreshCurrentUserAdminState());

        // Instead we allow for all:
        this._accessible = true;
    }


    get accessible(): boolean {
        return this._accessible;
    }

    get token(): string {
        return AppStorage.getItem<string>("token");
    }

    set token(val: string) {
        AppStorage.setItem("token", val);
    }

    get audience(): string {
        return AppStorage.getItem<string>("audience");
    }

    set audience(val: string) {
        AppStorage.setItem("audience", val);
    }

    refreshCurrentUserAdminState(): Promise<boolean> | boolean {
        return this._accessible;
    }
}