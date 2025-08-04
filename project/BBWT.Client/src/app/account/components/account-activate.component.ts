import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { MessageService } from "primeng/api";
import CryptoES from "crypto-es";

import { Message } from "@bbwt/classes";
import { UserPasswordSettings } from "@main/system-configuration";
import { AccountService } from "../services";
import { AccountActivationInfo } from "./../interfaces";
import {ActivationError} from "@account/enums/activation-error";


enum AccountActivateStatus {
    Loading = 0,
    FormReady = 1,
    OperationCompleted = 2,
    LoadUserError = 3,
    ActivationCodeInvalid = 4,
    ActivationCompleted = 5,
    InvitationNotFoundForUser = 6
}

@Component({
    selector: "account-activate",
    templateUrl: "./account-activate.component.html"
})
export class AccountActivateComponent implements OnInit {
    private code: string;

    status = AccountActivateStatus.Loading;
    email: string;
    userId: string;
    password: string;
    validCharacter: any = { lowercase: true, uppercase: true, numbers: false, special: false, minlength: 8 };

    get _accountActivateStatus(): typeof AccountActivateStatus {
        return AccountActivateStatus;
    }


    constructor(private router: Router,
                private accountService: AccountService,
                private route: ActivatedRoute,
                private messageService: MessageService) {
    }

    ngOnInit(): void {
        this.route.queryParams.subscribe(params => {
            this.userId = params["userId"];
            this.code = params["code"];
        });

        this.initSettings();
        this.getActivationInfo();
    }

    onSubmit(): void {
        const crpPassword = CryptoES.SHA512(this.password).toString();

        this.accountService.activateUser({ email: this.email, code: this.code, password: crpPassword }).then(() => {
            this.status = AccountActivateStatus.OperationCompleted;
            this.messageService.add(Message.Success("The account has been activated", "Activate Account"));
        }).catch((errorResponse: HttpErrorResponse) => {
            const error = errorResponse.error ? errorResponse.error : errorResponse.message;
            this.messageService.add(Message.Error(error, "Activate Account"));
        });
    }


    private getActivationInfo(): void {
        this.accountService.getAccountActivationInfo(this.userId, this.code)
            .then((info: AccountActivationInfo) => {
                if (info) {
                    this.email = info.email;
                    this.status = info.isInvited
                        ? AccountActivateStatus.FormReady
                        : AccountActivateStatus.OperationCompleted;
                }
            })
            .catch((error) => {
                switch (error.error) {
                    case ActivationError.ActivationCodeInvalid:
                        this.status = AccountActivateStatus.ActivationCodeInvalid
                        break;
                    case ActivationError.ActivationCompleted:
                        this.status = AccountActivateStatus.ActivationCompleted
                        break;
                    case ActivationError.InvitationNotFoundForUser:
                        this.status = AccountActivateStatus.InvitationNotFoundForUser
                        break;
                    default:
                        this.status = AccountActivateStatus.LoadUserError;
                        break;
                }
            });
    }

    private initSettings(): void {
        const settings = UserPasswordSettings.parse(this.route.snapshot.data["sysConfig"]);
        if (!settings) return;

        this.validCharacter = { minlength: settings.minPasswordLength, ...settings.validCharacters };

        // Trigger strong password directive update
        this.password = "";
    }
}