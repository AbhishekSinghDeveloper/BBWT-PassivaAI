import { Component, OnInit, ViewChild, Input, ViewChildren, QueryList, ChangeDetectorRef, OnDestroy } from "@angular/core";
import { DialogService, DynamicDialogRef } from "primeng/dynamicdialog";
import { DynamicDialogConfig } from "primeng/dynamicdialog";
import { AccountService } from "../../account/services";
import { FormGroup, FormBuilder, Validators } from "@angular/forms";

@Component({
    templateUrl: "./check-two-factor-code-dialog.component.html",
    styleUrls: ["./check-two-factor-code-dialog.component.scss"],
    // we need to inject it to every user component, otherwise if dialogs stack we get a bug: https://github.com/primefaces/primeng/issues/7077
    providers: [DialogService]
})
export class CheckTwoFactorCodeDialogComponent implements OnDestroy {
    userId: string;
    twoFactorCode: string;
    isSuccessful: boolean;

    callbacks: {
        onResolveAsCancel: () => void;
        onResolveAsOk: (dialogResult: any) => void;
    };

    constructor(
        public config: DynamicDialogConfig,
        public ref: DynamicDialogRef,
/*        public cdRef: ChangeDetectorRef,*/
        private accountService: AccountService) {
        Object.assign(this, config.data)
    }

    ngOnDestroy() {
        this.ref.close();
    }

    verify2faCode(): void {
        if (!this.twoFactorCode || this.twoFactorCode === "") return;

        this.accountService.checkTwoFactorCode(this.userId, this.twoFactorCode).then(() => {
            this.callbacks.onResolveAsOk(true);
        }).catch(() => {
            this.callbacks.onResolveAsCancel();
        });
        this.ref.close();
    }
}