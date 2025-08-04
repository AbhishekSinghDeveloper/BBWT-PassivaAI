import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { MessageService } from "primeng/api";
import CryptoES from "crypto-es";

import { Message } from "@bbwt/classes";
import { AccountService } from "../services";
import { UserService } from "@main/users";
import { UserPasswordSettings } from "@main/system-configuration";


export enum ResetPasswordStatus {
    Loading = 0,
    RecoveryCodeCheckFail = 1,
    UserEmailLoadingError = 2,
    FormReady = 3,
    OperationCompleted = 4,
    AuthenticatorAppStep = 5
}

@Component({
    selector: "reset-password",
    templateUrl: "./reset-password.component.html"
})
export class ResetPasswordComponent implements OnInit {
    private code: string;
    private reasonMessage: string;

    status = ResetPasswordStatus.OperationCompleted;
    validCharacter: any = { lowercase: true, uppercase: true, numbers: false, special: false, minlength: 8 };
    email: string;
    userId: string;
    password: string;
    twoFactorInfo: any;
    twoFactorCode: string;

    get resetPasswordStatusEnum(): any {
        return ResetPasswordStatus;
    }


    constructor(
        private route: ActivatedRoute,
        private messageService: MessageService,
        private userService: UserService,
        private accountService: AccountService) {}


    ngOnInit(): void {
        this.initSettings();

        this.route.queryParams.subscribe(params => {
            this.userId = params["userId"];
            this.code = params["code"];
            this.reasonMessage = params["reasonMessage"];
        });

        this.accountService.checkRecoveryCodeAvailability(this.userId, this.code).then(async data => {
            this.twoFactorInfo = await this.accountService.getTwoFactorInfo(this.userId);

            await this.loadUserEmail();
        }).catch((errorResponse: HttpErrorResponse) => {
            this.status = ResetPasswordStatus.RecoveryCodeCheckFail;
        });
    }

    onSubmit(): void {
        const crpPassword = CryptoES.SHA512(this.password).toString();

        this.accountService.resetPassword({ email: this.email, code: this.code, password: crpPassword }).then(() => {
            this.messageService.add(Message.Success("Password changed successfully", "Reset password"));
            this.status = ResetPasswordStatus.OperationCompleted;
        }).catch((errorResponse: HttpErrorResponse) => {
            const error = errorResponse.error ? errorResponse.error : errorResponse.message;
            this.messageService.add(Message.Error(error, "Reset password"));
        });
    }

    verify2faCode(): void {
        if (!this.twoFactorCode || this.twoFactorCode === "") return;

        this.accountService.checkTwoFactorCode(this.userId, this.twoFactorCode).then(() => {
            this.status = ResetPasswordStatus.FormReady;
        });
    }

    private loadUserEmail(): void {
        this.userService.getUserEmail(this.userId).then(email => {
            this.email = email;
            if (this.twoFactorInfo.twoFactorEnabled && this.twoFactorInfo.isRequireUserTwoFactorAuthenticationForSettings) {
                this.status = ResetPasswordStatus.AuthenticatorAppStep;
            } else {
                this.status = ResetPasswordStatus.FormReady;
            }
        }).catch(() => {
            this.status = ResetPasswordStatus.UserEmailLoadingError;
        });
    }

    private initSettings(): void {
        const settings = UserPasswordSettings.parse(this.route.snapshot.data["sysConfig"]);
        if (!settings) return;

        this.validCharacter = { minlength: settings.minPasswordLength, ...settings.validCharacters };

        this.password = "";
    }
}