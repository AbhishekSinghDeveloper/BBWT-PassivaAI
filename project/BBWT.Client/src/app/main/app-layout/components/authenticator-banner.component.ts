import { Component, OnInit } from "@angular/core";
import { IUser, UserService } from "@main/users";

@Component({
    selector: "authenticator-banner",
    templateUrl: "./authenticator-banner.component.html",
    styleUrls: ["./authenticator-banner.component.scss"]
})
export class AuthenticatorBannerComponent implements OnInit {
    user: IUser;
    showAuthenticatorBanner = true;

    constructor(private userService: UserService) {}

    ngOnInit() {
        this.user = this.userService.currentUser;
        if (!(this.userService.currentUser.twoFactorEnabled || this.userService.currentUser.u2fEnabled)) {
            this.showAuthenticatorBanner = this.userService.currentUser.roles.some(x => x.authenticatorRequired == true);
        }
    }
}