import { Component, OnDestroy, OnInit } from "@angular/core";

import { Subscription } from "rxjs/index";

import { CurrentUserChangedData, IUser, UserService } from "@main/users";
import { ImpersonationService } from "./impersonation.service";
import { BroadcastService } from "@bbwt/modules/broadcasting";


@Component({
    templateUrl: "./impersonation.component.html",
    styleUrls: ["./impersonation.component.scss"]	
})
export class ImpersonationComponent implements OnInit, OnDestroy {
    private subscription: Subscription;

    demoManager: IUser = {} as IUser;
    demoUser: IUser = {} as IUser;
    isDemoManager: boolean;
    isDemoUser: boolean;
    isImpersonating = false;
    gettingImpersonateState = false;


    constructor(private impersonationService: ImpersonationService,
                private broadcastService: BroadcastService,
                private userService: UserService) {
        this.subscription = broadcastService.on<CurrentUserChangedData>(UserService.CurrentUserChangedEventName).subscribe(data => {
            if (!data.isLogout) {
                this.refreshUserImpersonationState();
            } else {
                this.isImpersonating = false;
            }
        });
    }


    ngOnInit(): void {
        this.init();
    }

    ngOnDestroy(): void {
        this.subscription.unsubscribe();
    }


    impersonateAsDemoManager(): void {
        this.impersonateAsUser(this.demoManager);
    }

    impersonateAsDemoUser(): void {
        this.impersonateAsUser(this.demoUser);
    }

    stopImpersonation(): void {
        this.userService.stopImpersonationForCurrentUser();
    }

    isSystemAdmin(): boolean {
        return this.userService.currentUser.isSystemAdmin;
    }


    private async init(): Promise<void> {
        this.demoManager = await this.impersonationService.getImpersonatedDemoManager();
        this.demoUser = await this.impersonationService.getImpersonatedDemoUser();

        this.refreshUserImpersonationState();
    }

    private impersonateAsUser(user: IUser): void {
        this.userService.impersonateCurrentUserAsUser(user.id);
    }

    private refreshUserImpersonationState(): void {
        this.gettingImpersonateState = true;
        this.isDemoManager = this.userService.currentUser.email === this.demoManager.email;
        this.isDemoUser = this.userService.currentUser.email === this.demoUser.email;

        this.userService.isCurrentUserImpersonating().then((resultData: any) => {
            if (resultData != null) {
                this.isImpersonating = resultData.isImpersonating;
            }
        }).finally(() => this.gettingImpersonateState = false);
    }
}