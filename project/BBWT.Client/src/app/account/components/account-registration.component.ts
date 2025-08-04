import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";

import { MessageService } from "primeng/api";
import CryptoES from "crypto-es";

import { Message } from "@bbwt/classes";
import { AuthenticationSettings } from "../interfaces";
import { AccountService } from "../services";
import { UserPasswordSettings, SystemConfigurationResolveData } from "@main/system-configuration";
import { ValidationPatterns } from "@bbwt/modules/validation";


export enum AccountRegistrationStatus {
    FormReady,
    ApprovalRequired,
    ConfirmationSent,
    OperationCompleted
}

@Component({
    selector: "account-registration",
    templateUrl: "./account-registration.component.html"
})
export class AccountRegistrationComponent implements OnInit {
    user = {email: "", firstname: "", lastname: "", password: "", confirmPassword: ""};
    passwordValidationSettings: any = {lowercase: true, uppercase: true, numbers: false, special: false, minlength: 8};
    showPwnedWarning = false;
    antiAutocomplete = true;
    authenticationSettings: AuthenticationSettings = {} ; // Required for AutoLogin
    breaches: number;
    status = AccountRegistrationStatus.FormReady;

    get validationPatterns() {
        return ValidationPatterns; 
    }
    get accountRegistrationStatusEnum() {
         return AccountRegistrationStatus;
    }
    get emailAsPassword(): boolean {
        return this.user.password && this.user.email?.toLowerCase() == this.user.password?.toLowerCase();
    }


    constructor(private router: Router,
                private route: ActivatedRoute,
                private accountService: AccountService,
                private messageService: MessageService) { }


    ngOnInit(): void {
        this.initSettings();
    }

    onSubmit(): void {
        const crpPassword = CryptoES.SHA512(this.user.password).toString();
        const crpPasswordSHA1 = CryptoES.SHA1(this.user.password).toString();

        const userObj = {
            email: this.user.email,
            firstName: this.user.firstname,
            lastName: this.user.lastname,
            password: crpPassword,
            passwordSHA1: crpPasswordSHA1
        };

        this.accountService.registerAccount(userObj).then(res => {
            this.messageService.add(Message.Success("The user has been registered successfully.", "User Registration"));

            if (res.pwnedResult > 0) {
                this.breaches = res.pwnedResult;
                this.showPwnedWarning = true;
            }

            if (res.adminApprovalRequired) this.status = AccountRegistrationStatus.ApprovalRequired;
            if (res.confirmationSent) this.status = AccountRegistrationStatus.ConfirmationSent;
        });
    }


    private initSettings(): void {
        const config = this.route.snapshot.data["sysConfig"];

        this.setPasswordSettings(config);
    }

    private setPasswordSettings(config: SystemConfigurationResolveData): void {
        const settings = UserPasswordSettings.parse(config);
        if (!settings) return;

        this.passwordValidationSettings = { minlength: settings.minPasswordLength, ...settings.validCharacters };
        this.antiAutocomplete = !settings.autocomplete;

        // Trigger strong password directive update
        this.user.password = "";

        this.status = AccountRegistrationStatus.FormReady;
    }
}