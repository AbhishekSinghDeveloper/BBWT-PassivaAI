import { HttpClient, HttpErrorResponse, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService } from "@bbwt/modules/data-service/base.data.service";
import {
    HttpResponsesHandlersFactory
} from "@bbwt/modules/data-service/http-responses-handler";
import {
    AuthenticationSettings, U2fAuthenticationResponse, U2fRegistrationRequest, U2fRegistrationResponse, RecoverPassword, ISignUpResult
} from "../interfaces";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { LoginRequest } from "@account/interfaces/login-request";
import { IUser } from "@main/users";
import { RecoveryCode } from "@account/interfaces/recovery-code";
import { AppStorage } from "@bbwt/utils/app-storage";

@Injectable({
    providedIn: "root"
})
export class AccountService extends BaseDataService {
    readonly url = "api/account";

    static readonly TwoFactorEnabledEventName = "TwoFactorEnabled";
    static readonly UserLoginEventName = "UserLogin";
    static readonly UserLogoutEventName = "UserLogout";

    private loginPromise: Promise<AuthenticationSettings>;
    private logoutPromise: Promise<void>;

    constructor(protected http: HttpClient,
                protected handlersFactory: HttpResponsesHandlersFactory,
                private broadcastService: BroadcastService
    ) {
        super(http, handlersFactory);
    }

    login(request: LoginRequest): Promise<AuthenticationSettings> {
        if (!this.loginPromise) {
            this.loginPromise = this.httpPost<AuthenticationSettings>("login", request)
                .then(authenticationSettings => {
                    this.loginPromise = null;

                    if (authenticationSettings.loggedUser) {
                        this.broadcastService.broadcast(AccountService.UserLoginEventName, authenticationSettings.loggedUser);
                    }

                    return authenticationSettings;
                })
                .catch((errorResponse) => {
                    this.loginPromise = null;
                    throw errorResponse;
                });
        }
        return this.loginPromise;
    }

    logout(): Promise<void> {
        if (!this.logoutPromise) {
            this.logoutPromise = this.httpPost("logout")
                .then(() => {
                    this.logoutPromise = null;
                    this.broadcastService.broadcast(AccountService.UserLogoutEventName);
                    AppStorage.setItem(AppStorage.ImpersonationDataKey, null);
                })
                .catch((response: HttpErrorResponse) => {
                    if (response.status == 401) {
                        this.broadcastService.broadcast(AccountService.UserLogoutEventName);
                        AppStorage.setItem(AppStorage.ImpersonationDataKey, null);
                    }

                    this.logoutPromise = null;
                });
        }

        return this.logoutPromise;
    }

    registerAccount(userObj: any): Promise<ISignUpResult> {
        return this.httpPost("register", userObj);
    }

    resetPassword(data: any): Promise<any> {
        return this.httpPost("reset-password", data, this.noHandler);
    }

    activateUser(data: any): Promise<any> {
        return this.httpPost("activate", data, this.noHandler);
    }

    confirmEmail(data: any): Promise<any> {
        return this.httpPost("confirm-email", data, this.noHandler);
    }

    recoverPassword(data: RecoverPassword): Promise<any> {
        return this.httpPost("recover-password", data, this.noHandler);
    }

    getReCaptchaSiteKey(): Promise<string> {
        return this.httpGet("recaptcha-site-key", this.noHandler);
    }

    checkIfIpIsBlocked(): Promise<any> {
        return this.httpGet("ip-locking-time");
    }
    enableAuthenticator(): Promise<any> {
        return this.httpGet("me/2fa-enabling-data");
    }

    verificationCode(data: any): Promise<any> {
        return this.httpPost("me/enable-2fa", data);
    }

    enableU2F(): Promise<any> {
        return this.httpPost("me/enable-u2f");
    }

    disableU2F(): Promise<any> {
        return this.httpPost("me/disable-u2f");
    }

    disable2fa(code: string): Promise<void> {
        return this.httpPost("me/disable-2fa", { code });
    }

    get2faU2fRecoveryCodes(isNeedNew?: boolean): Promise<string> {
        if (isNeedNew) {
            return this.httpGet(`me/2fa-u2f-recovery-codes/${isNeedNew}`);
        } else {
            return this.httpGet("me/2fa-u2f-recovery-codes");
        }
    }

    generateServerChallenge(): Promise<U2fRegistrationRequest> {
        return this.httpGet("me/generate-u2f-device-registration-challenge");
    }

    registerU2fDevice(deviceResponse: U2fRegistrationResponse): Promise<any> {
        return this.httpPost("me/register-u2f-device", deviceResponse);
    }

    authenticateU2fDevice(deviceResponse: U2fAuthenticationResponse): Promise<any> {
        return this.httpPost("authenticate-u2f-device", deviceResponse);
    }

    loginWithRecoveryCode(data: RecoveryCode): Promise<IUser> {
        return this.httpPost<IUser>("recovery-code-login", data)
            .then(loggedUser => {
                this.broadcastService.broadcast(AccountService.UserLoginEventName, loggedUser);
                return loggedUser;
            });
    }

    checkRecoveryCodeAvailability(userId: string, recoveryCode: string): Promise<any> {
        return this.handle(this.http.get(`${this.url}/check-recovery-code`,
            { params: new HttpParams({ fromObject: { userId, code: recoveryCode } }) })
        );
    }

    getToken(): Promise<any> {
        return this.httpGet("token");
    }

    getAccountActivationInfo(userId: string, code: string): Promise<any> {
        return this.handle(this.http.get(`${this.url}/activation-info`,
            { params: new HttpParams({ fromObject: { userId, code } }) })
        );
    }

    getTwoFactorInfo(userId: string): Promise<any> {
        return this.handle(this.http.get(`${this.url}/two-factor-info`,
            { params: new HttpParams({ fromObject: { userId } }) })
        );
    }

    checkTwoFactorCode(userId: string, code: string): Promise<any> {
        return this.handle(this.http.get(`${this.url}/check-two-factor-code`,
            { params: new HttpParams({ fromObject: { userId, code } }) })
        );
    }

}