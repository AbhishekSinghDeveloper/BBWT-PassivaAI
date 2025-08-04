import { Component, OnInit } from "@angular/core";
import { register } from "@nomost/u2f-api";
import { UserService, IUser } from "../users";
import { AccountService } from "@account/services";
import { MessageService } from "primeng/api";
import { Message } from "@bbwt/classes";
import { U2fRegistrationRequest, U2fRegistrationResponse } from "@account/interfaces";
import { DialogService, DynamicDialogConfig } from "primeng/dynamicdialog";
import { CheckTwoFactorCodeDialogComponent } from "./check-two-factor-code-dialog.component";

@Component({
    templateUrl: "./authentication.component.html",
    styleUrls: ["./authentication.component.scss"],
    providers: [DialogService]
})
export class AuthenticationComponent implements OnInit {
    showAuthFactorModal = false;
    showU2fModal = false;
    user: IUser = {} as any;
    deviceResponse: U2fRegistrationResponse;
    authenticatorUri: string = null;
    sharedKey: string;
    code: string;
    recoveryCode: string;
    deviceRegistered = false;
    disable2FACode: string;
    isShowRecoveryCode = false;

    constructor(
        private readonly accountService: AccountService,
        readonly userService: UserService,
        private readonly messageService: MessageService,
        private dialogService: DialogService,
    ) {}

    ngOnInit(): void {
        this.userService.getLoggedUser().then((user: IUser) => {
            this.user = user;

            if (this.user.twoFactorEnabled) {
                this.accountService.get2faU2fRecoveryCodes().then(recoveryCode => this.recoveryCode = recoveryCode);
            }
        });
    }

    async disableU2F(): Promise<void> {
        try {
            await this.accountService.disableU2F();
            this.user.u2fEnabled = false;
            this.showU2fModal = false;
            await this.userService.refreshCurrentUser();
            this.messageService.add(Message.Success("U2F successfully disabled."));
        } catch {
            this.messageService.add(Message.Error("Failed to disable U2F."));
        }
    }

    getDeviceResponse(): void {
        this.accountService.generateServerChallenge().then((serverChallenge: U2fRegistrationRequest) => {
            register(serverChallenge, 10000)
                .then((deviceResponse: U2fRegistrationResponse) => this.deviceResponse = deviceResponse)
                .catch(error => {
                    this.messageService.add(Message.Error(error, "Failed to get the device response."));
                });
        }).catch(() => {
            this.messageService.add(Message.Error("Failed to generate server challenge."));
        });
    }

    async sendDeviceResponse():  Promise<void> {
        try {
            await this.accountService.registerU2fDevice(this.deviceResponse);
            if (!this.user.u2fEnabled) {
                try {
                    const recoveryCode =  await this.accountService.enableU2F();
                    this.recoveryCode = recoveryCode;
                    this.user.u2fEnabled = true;
                    await this.userService.refreshCurrentUser();
                    this.messageService.add(Message.Success("U2F successfully enabled."));
                } catch {
                    this.messageService.add(Message.Error("Failed to enable U2F"));
                }
            } else {
                this.messageService.add(Message.Success("Security Key successfully added."));
            }
        } catch {
            this.messageService.add(Message.Error("Device response rejected by the server."));
        }
    }

    enableAuthenticator(): void {
        this.accountService.enableAuthenticator().then(d => {
            this.sharedKey = d.sharedKey;
            this.authenticatorUri = d.authenticatorUri;
        });
    }

    async verifyCode(): Promise<void> {
        if (!this.code || this.code === "") return;

        const data = { Code: this.code };

        this.recoveryCode = await this.accountService.verificationCode(data);
        this.user.twoFactorEnabled = true;
        await this.userService.refreshCurrentUser();
        this.messageService.add(Message.Success("Authenticator successfully enabled."));
    }

    async disable2FA(): Promise<void> {
        await this.accountService.disable2fa(this.disable2FACode);
        this.disable2FACode = null;
        this.recoveryCode = null;
        this.user.twoFactorEnabled = false;
        await this.userService.refreshCurrentUser();

        // to reload the 2fa enabling data after disable it (when popup won't close this is necessary)
        this.enableAuthenticator();

        this.messageService.add(Message.Success("Authenticator successfully disabled."));
    }

    async check2fa() {
        this.isShowRecoveryCode = await this.open2faChekingDialog();
    }

    async showAuthFactorDialog() {
        this.showAuthFactorModal = true;

        const twoFactorInfo = await this.accountService.getTwoFactorInfo(this.user.id);
        this.isShowRecoveryCode = !twoFactorInfo.isRequireUserTwoFactorAuthenticationForSettings;
    }

    async getNewRecoveryCode() {
        const twoFactorInfo = await this.accountService.getTwoFactorInfo(this.user.id);
        if (twoFactorInfo.isRequireUserTwoFactorAuthenticationForSettings) {
            if (await this.open2faChekingDialog()) {
                const recoveryCode = await this.accountService.get2faU2fRecoveryCodes(true);
                this.recoveryCode = recoveryCode;
            }
        } else {
            const recoveryCode = await this.accountService.get2faU2fRecoveryCodes(true);
            this.recoveryCode = recoveryCode;
        }
    }

    private async open2faChekingDialog() {
        return new Promise<any>((resolve, reject) => {
            const config: DynamicDialogConfig = {
                header: "2FA Checking",
                width: "30%"
            };
            config.data = {
                userId: this.user.id
            }

            config.data.callbacks = {
                onResolveAsCancel: () => {
                    resolve(false);
                },
                onResolveAsOk: (dialogResult) => {
                    resolve(dialogResult);
                }
            };

            this.dialogService.open(CheckTwoFactorCodeDialogComponent, config);

            // fixes weird bug when after opening DynamicDialog focus stays on last clicked element (button that opens that dialog)
            // and when user presses enter/spacebar this will trigger keypress and reopens this DynamicDialog
            setTimeout(() => {
                try {
                    const focusedElem = document.activeElement as HTMLElement;
                    focusedElem.blur();
                } catch { }
            }, 0);
        });
    }
}