import { Router } from "@angular/router";
import { Component, OnInit } from "@angular/core";

import { MenuItem } from "primeng/api";

import { BroadcastService } from "@bbwt/modules/broadcasting";
import { UserService } from "@main/users";


@Component({
    templateUrl: "user-profile.component.html",
})
export class UserProfileComponent implements OnInit {
    initialTabIndex: number;
    items: MenuItem[];

    constructor(public router: Router, public userService: UserService, public broadcastService: BroadcastService) {
        this.initialTabIndex = this.router.url.endsWith("authentication") ? 1 : 0;
        broadcastService.on(UserService.CurrentUserChangedEventName).subscribe(() => {
            this.items = this.generateMenuItems();
        });
    }


    ngOnInit(): void {
        this.items = this.generateMenuItems();
    }


    generateMenuItems(): MenuItem[] {
        return [
            { label: "Personal Information",  routerLink: ["./"], disabled: this.userService.isUserRequiredSetupTwoFactor },
            { label: "Authentication",  routerLink: ["authentication"] }
        ];
    }
}