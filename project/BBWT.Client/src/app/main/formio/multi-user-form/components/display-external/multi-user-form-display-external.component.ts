import {Component, OnInit} from "@angular/core";
import {UserService} from "@main/users";
import {ActivatedRoute, Params} from "@angular/router";
import {MultiUserFormAssociationsService} from "../../services/multi-user-form-associations.service";
import {MultiUserFormDisplayComponent} from "../display/multi-user-form-display.component";
import {AccountService} from "@account/services";

@Component({
    selector: "multi-user-form-display",
    templateUrl: "./multi-user-form-display-external.component.html",
    styleUrls: ["./multi-user-form-display-external.component.scss"],
    providers: [MultiUserFormAssociationsService]
})
export class MultiUserFormDisplayExternalComponent extends MultiUserFormDisplayComponent implements OnInit {
    email: string;
    passCode: string;
    wrongStage = false;
    accessDenied = false;
    params: Params;

    constructor(private accountService: AccountService,
                multiUserFormAssociationsService: MultiUserFormAssociationsService,
                userService: UserService,
                activatedRoute: ActivatedRoute) {
        super(multiUserFormAssociationsService, userService, activatedRoute);
    }

    async ngOnInit() {
        this.activatedRoute.queryParams.subscribe(async params => {
            this.params = params;
            await this.validateCredentials();
        });
    }

    async formSaved() {
        setTimeout(() => {
            this.accountService.logout();
        }, 2000);

    }

    async validateCredentials() {
        try {
            await this.processingParams(this.params, this.email);
        } catch (error) {
            console.log(error)
        }
        if (!this.muFormAssoc?.multiUserFormAssociationLinks[0].externalUserEmail) {
            this.wrongStage = true;
            this.formReady = true;
        }
        if (this.target != this.userService.currentUser.email ||
            this.muFormAssoc?.multiUserFormAssociationLinks[0].externalUserEmail != this.userService.currentUser.email) {
            this.accessDenied = true;
            this.formReady = true;
        }
        if (!this.formReady || this.formFilled) {
            this.formSaved().then();
        }
    }
}