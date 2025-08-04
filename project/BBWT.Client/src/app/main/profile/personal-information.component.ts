import { HttpErrorResponse, HttpEventType } from "@angular/common/http";
import { Component, ViewChild } from "@angular/core";

import CryptoES from "crypto-es";
import * as moment from "moment";
import { MessageService } from "primeng/api";
import { FileUpload } from "primeng/fileupload";

import { RecoverPassword } from "@account/interfaces";
import { AccountService } from "@account/services";
import { Router } from "@angular/router";
import { Message } from "@bbwt/classes";
import { IRealUser } from "@bbwt/interfaces";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { AppStorage } from "@bbwt/utils/app-storage";
import { FileDetails } from "../file-storage/file-details";
import { IUser, UserService } from "../users";
import { AccountStatus } from "../users/account-status";
import { PictureMode } from "../users/picture-mode";
import { DialogService, DynamicDialogConfig, DynamicDialogRef } from "primeng/dynamicdialog";
import { CheckTwoFactorCodeDialogComponent } from "./check-two-factor-code-dialog.component";

@Component({
    templateUrl: "./personal-information.component.html",
    styleUrls: ["./personal-information.component.scss"],
    providers: [DialogService]
})
export class PersonalInformationComponent {
    user: IUser;
    realUser: IRealUser;
    originalFormData: string;

    get _pictureModeEnum() {
        return PictureMode;
    }
    get _accountStatusEnum() {
        return AccountStatus;
    }
    get validationPatterns(): any {
        return ValidationPatterns;
    }

    @ViewChild("avatarImageUploader", { static: false }) avatarImageUploader: FileUpload;

    constructor(
        private userService: UserService,
        private accountService: AccountService,
        private messageService: MessageService,
        private router: Router,
        private dialogService: DialogService,
    ) {
        this.initData();
    }

    async save() {
        if (this.user.gravatarEmail) {
            this.user.gravatarImage =
                "https://www.gravatar.com/avatar/" + CryptoES.MD5(this.user.gravatarEmail).toString();
        }

        if (this.user.twoFactorEnabled) {
            const originalValue = this.getOriginalValue();
            const twoFactorInfo = await this.accountService.getTwoFactorInfo(this.user.id);
            if (twoFactorInfo.isRequireUserTwoFactorAuthenticationForSettings && originalValue.email != this.user.email) {
                await this.open2faChekingDialog();
            }
        }

        // Saving Real User
        AppStorage.setItem(AppStorage.RealUserKey, this.realUser);

        // Saving User
        try {
            await this.userService.updateLoggedUser(this.user);
            this.messageService.add(Message.Success("Profile successfully updated.", "Update Profile"));
        } catch (e: unknown) {
            if (e instanceof HttpErrorResponse && e.error?.newEmail) {
                this.messageService.add(Message.Error(
                    "You need to confirm your new email before continuing using the App.",
                    "Update Profile"));
            }
        }
    }

    switchUsingDefaultEmail(event: boolean): void {
        if (event) {
            this.user.gravatarEmail = this.user.email;
        } else {
            this.user.gravatarEmail = null;
        }
    }

    sendPasswordResetMail(): void {
        this.accountService
            .recoverPassword({ email: this.userService.currentUser.email } as RecoverPassword)
            .then(() => {
                this.messageService.add(Message.Success("The email has been sent.", "Reset Password"));
            });
    }

    addorUpdateSignature(): void {

        const queryParams = { token: this.userService?.currentUser?.id ?? "" };
        const url = this.router.serializeUrl(
            this.router.createUrlTree(["app/formio/usersignature"], { queryParams }));

        window.open(url, "_blank");


    }

    avatarImageUploading(event) {
        if (event.files !== undefined && event.files.length > 0) {
            for (const file of event.files) {
                const formData = new FormData();
                formData.append(file.name, file);
                formData.append("last_modified", moment(file.lastModified).toDate().toUTCString());

                this.userService.uploadAvatarImage(formData).subscribe({
                    next: e => {
                        switch (e.type) {
                            case HttpEventType.Response:
                                const resFile = e.body as FileDetails;
                                if (resFile) {
                                    this.user.avatarImage = resFile;
                                    this.user.avatarImageId = <string>resFile.id;
                                }
                                this.avatarImageUploader.clear();
                        }
                    },
                    error: errorResponse => {
                        this.messageService.add(Message.Error(errorResponse.error, "Logo Image Uploading"));
                        this.avatarImageUploader.clear();
                    }
                });
            }
        }
    }

    onAvatarImageUploadingError(event) {
        let errorDescripton = "The following files were not be uploaded: <br/>";
        if (event.files.length > 0) {
            for (const file of event.files) {
                errorDescripton += file.name + "<br/>";
            }
        }
        this.messageService.add(
            Message.Error(
                "An error occurred while uploading files. Please try again.<br/>" + errorDescripton,
                "Avatar Image Uploading"
            )
        );
    }

    clearAvatarImage(): void {
        this.user.avatarImage = null;
        this.user.avatarImageId = null;
    }

    updateOriginalValue() {
        this.originalFormData = JSON.stringify(this.user);
    }

    getOriginalValue(): IUser {
        return JSON.parse(this.originalFormData) as IUser;
    }

    async open2faChekingDialog() {

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
                    resolve(null);
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

    private initData(): void {
        // Request Current User
        this.user = this.userService.currentUser;

        // Loading Real User
        const realUser = AppStorage.getItem<IRealUser>(AppStorage.RealUserKey);
        this.realUser = realUser ? realUser : ({} as any);

        // Save original data
        this.updateOriginalValue();
    }
}
