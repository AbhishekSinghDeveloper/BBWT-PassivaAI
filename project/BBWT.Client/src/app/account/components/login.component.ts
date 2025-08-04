import { Component, Inject, OnDestroy, OnInit } from "@angular/core";
import { DOCUMENT } from "@angular/common";
import { Router, ActivatedRoute } from "@angular/router";
import { MessageService } from "primeng/api";
import { ReCaptchaV3Service } from "ngx-captcha";
import { sign } from "@nomost/u2f-api";
import CryptoES from "crypto-es";

import { AuthenticationSettings, U2fAuthenticationResponse } from "../interfaces";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { UserPasswordSettings } from "@main/system-configuration";
import { AccountService } from "../services";
import { UserService } from "@main/users";
import { AppStorage } from "@bbwt/utils/app-storage";
import { Message } from "@bbwt/classes";
import { IRealUser } from "@bbwt/interfaces";
import { SsoProviderService } from "@main/sso-provider";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { AppOnlineStateService } from "@main/pwa/app-online-state.service";
import { LoginState } from "@account/enums/login-state";
import { LoginRequest } from "@account/interfaces/login-request";
import { Subscription } from "rxjs";
import { BrowserInfoService } from "@bbwt/services/browser-info.service";
import { ResetPasswordRequest } from "../interfaces/reset-password-request";
import { ResetPasswordRequestReason } from "../enums/reset-password-reason";

@Component({
    selector: "login",
    templateUrl: "./login.component.html",
    styleUrls: ["./login.component.scss"]
})
export class LoginComponent implements OnInit, OnDestroy {
    loginRequest: LoginRequest = {} as LoginRequest;
    authenticationSettings: AuthenticationSettings = {};
    antiAutocomplete = true;
    reCaptchaSiteKey = "";
    sustainsysSamlEndpoint: string;
    loginBtnDisabled = false;
    twoStepLoginDialogVisible = false;
    loginMessage = "";
    loginState = LoginState.LogPasswordStep;
    recaptcha: any = null;
    loginLockoutTimeoutInSeconds = 0;
    lockType = "";
    redirectUrl: string;

    private _formReady = false;
    private _subscriptions = new Array<Subscription>();
    private _timerId: NodeJS.Timeout;

    constructor(
        public ssoProviderService: SsoProviderService,
        public appOnlineStateService: AppOnlineStateService,
        private router: Router,
        private route: ActivatedRoute,
        private accountService: AccountService,
        private messageService: MessageService,
        private userService: UserService,
        private broadcastService: BroadcastService,
        private reCaptchaV3Service: ReCaptchaV3Service,
        @Inject(DOCUMENT) private document: Document,
        private browserInfo: BrowserInfoService
    ) {}

    get validationPatternsType(): typeof ValidationPatterns {
        return ValidationPatterns;
    }

    get loginStateType(): typeof LoginState {
        return LoginState;
    }

    // The flag is used to avoid multiple passes of form drawing, in order to make all element shown at once
    get componentReady(): boolean {
        return this._formReady || this.authenticationSettings.isSystemTester;
    }

    get hasTwoFactor(): boolean {
        const { u2fEnabled, authenticatorEnabled } = this.authenticationSettings;
        return u2fEnabled || authenticatorEnabled;
    }

    get isCaptchaActive(): boolean {
        return !!this.reCaptchaSiteKey;
    }

    async ngOnInit() {
        this.initSettings();

        this.route.queryParams.subscribe(params => {
            this.redirectUrl = params["redirectStr"];
        });

        this.loginMessage = AppStorage.getItemFromSession<string>(
            AppStorage.LogoutReasonMessageKey
        );
        if (this.loginMessage) {
            AppStorage.setItemForSession(AppStorage.LogoutReasonMessageKey, null);
        }
        if (this.userService.isLogged) {
            await this.goToApp();
        }

        this.reCaptchaSiteKey = await this.accountService.getReCaptchaSiteKey();
        this.sustainsysSamlEndpoint =
            this.document.location.protocol +
            "//" +
            this.document.location.hostname +
            ":" +
            this.document.location.port +
            "/sustainsys-saml";

        const response = await this.accountService.checkIfIpIsBlocked();
        if (response) {
            this.disableLoginButton(response);
            this.messageService.add(
                Message.Error(
                    `Please, try again in "${response}" seconds". "Too many attempts to login. User is locked out.`
                )
            );
        }

        const realUser = AppStorage.getItem<IRealUser>(AppStorage.RealUserKey);
        if (realUser) {
            this.loginRequest.realEmail = realUser.email;
            this.loginRequest.realFirstName = realUser.firstName;
            this.loginRequest.realLastName = realUser.lastName;
        }

        this._subscriptions.push(
            this.broadcastService
                .on<void>(AccountService.TwoFactorEnabledEventName)
                .subscribe(() => this.goToApp())
        );
    }

    ngOnDestroy(): void {
        this._subscriptions.forEach(x => x?.unsubscribe());
    }

    async onKey(event): Promise<void> {
        if (event.keyCode === 13 && !this.loginBtnDisabled) {
            await this.login();
        }
    }

    disableLoginButton(durationSeconds: number): void {
        this.loginBtnDisabled = true;
        setTimeout(() => {
            this.loginBtnDisabled = false;
        }, +durationSeconds * 1000);
    }

    async login(): Promise<void> {
        this.loginRequest.captchaResponse = "";
        if (this.isCaptchaActive) {
            this.loginRequest.captchaResponse = await this.reCaptchaV3Service.executeAsPromise(
                this.reCaptchaSiteKey,
                "login",
                { useGlobalDomain: false }
            );
        }

        const request = { ...this.loginRequest };
        request.password = CryptoES.SHA512(this.loginRequest.password).toString();
        request.browser = this.browserInfo.browserId;
        request.fingerprint = this.browserInfo.browserFingerprint;
        this.loginMessage = "";
        this.disableLoginButton(1);

        AppStorage.setItem(AppStorage.RealUserKey, {
            firstName: this.loginRequest.realFirstName,
            lastName: this.loginRequest.realLastName,
            email: this.loginRequest.realEmail
        } as IRealUser);

        const settings = await this.loginUser(request);

        if (settings.lockoutUserEnabled || settings.lockoutIpEnabled) {
            this.lockType = settings.lockoutUserEnabled ? "User" : "IP Address";
            this.lockLogin(settings.lockoutTimeoutInSeconds);
            return;
        }

        if (settings.passwordResetRequired) {
            this.goToResetPassword(settings.userId, settings.passwordResetRequest);
            return;
        }

        if (settings.isSystemTester) return;

        if (settings.loggedUser) {
            await this.goToApp();
            return;
        }

        await this.twoFactorAuthentication(settings);
    }

    goToResetPassword(userId: string, request: ResetPasswordRequest) {
        let reasonMessage = "";
        switch (request.reason) {
            case ResetPasswordRequestReason.initialAccountReset:
                reasonMessage =
                    "This appears to be a new live environment. When commissioning and deploying a new environment, "
                    + "site administrators must set a new password to protect the system. Do not re-use passwords. "
                    + "If you are not responsible for deploying the site to a new environment, then you are seeing this "
                    + "message in error – please urgently contact your system administrator.";
                break;
        }
        this.router.navigateByUrl(
            `/account/resetpassword?userId=${userId}&code=${request.passwordResetCode}&reasonMessage=${reasonMessage}`);
    }

    async submitDeviceResponse(deviceResponse: U2fAuthenticationResponse): Promise < void> {
        await this.accountService.authenticateU2fDevice({
            ...deviceResponse,
            userId: this.authenticationSettings.userId
        });

        await this.login();
    }

    async submitRecoveryCode(): Promise<void> {
        this.authenticationSettings.loggedUser = await this.accountService.loginWithRecoveryCode({
            browser: this.browserInfo.browserId,
            userId: this.authenticationSettings.userId,
            code: this.loginRequest.twoFactorRecoveryCode,
            fingerprint: this.browserInfo.browserFingerprint
        });

        await this.userService.refreshCurrentUser();
        if (!this.userService.isUserRequiredSetupTwoFactor) {
            await this.goToApp();
            return;
        }

        this.loginState = LoginState.EnableAuthenticatorStep;
    }

    private lockLogin(timeoutInSeconds: number): void {
        if (this._timerId) return;

        this.loginLockoutTimeoutInSeconds = timeoutInSeconds;
        this.loginState = LoginState.LoginLockedStep;

        this._timerId = setInterval(() => {
            this.loginLockoutTimeoutInSeconds--;
            if (this.loginLockoutTimeoutInSeconds <= 0) {
                clearInterval(this._timerId);
                this.loginState = LoginState.LogPasswordStep;
                this._timerId = null;
            }
        }, 1000);
    }

    private async loginUser(request: LoginRequest): Promise<AuthenticationSettings> {
        let settings: AuthenticationSettings = {};
        try {
            settings = await this.accountService.login(request);
            if (settings.isNewBrowserLogin) {
                this.showNewBrowserLoginAlert();
            }
        } catch (ex) {
            this.loginMessage = ex.error;
            throw ex;
        }
        // Full page reload to bootstrap using app.module as an entry point
        const { u2fAuthenticationRequest, ...rest } = settings;
        this.authenticationSettings = rest;
        return settings;
    }

    private showNewBrowserLoginAlert() {
        setTimeout(() => {
            this.messageService.add(
                Message.Info(
                    "You appear to be connecting from a different browser than the one you used " +
                    "previously. If this is a surprise to you, please consider changing your password."
                )
            );
        }, 500);
    }

    private async twoFactorAuthentication(settings: AuthenticationSettings): Promise<void> {
        if (settings.u2fEnabled) {
            this.loginState = LoginState.U2fStep;
            const deviceResponse = await sign(settings.u2fAuthenticationRequest, 150000);
            return await this.submitDeviceResponse(deviceResponse as U2fAuthenticationResponse);
        }

        this.loginState = LoginState.AuthenticatorAppStep;
    }

    private goToApp(): Promise<boolean> {
        return this.router.navigateByUrl(this.redirectUrl ?? "/app");
    }

    private initSettings(): void {
        const settings = UserPasswordSettings.parse(this.route.snapshot.data["sysConfig"]);
        if (!settings) return;

        this.antiAutocomplete = !settings.autocomplete;
        this._formReady = true;
    }
}
