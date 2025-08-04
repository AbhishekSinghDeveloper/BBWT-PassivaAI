import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { AccountService } from "../services";

export enum ConfirmEmailStatus {
    Loading = 0,
    OperationCompleted = 1,
    Error = 2
}

@Component({
    selector: "confirm-email",
    templateUrl: "./confirm-email.component.html"
})
export class ConfirmEmailComponent implements OnInit {
    private code: string;

    get confirmEmailStatus(): any {
        return ConfirmEmailStatus; 
    }

    status: ConfirmEmailStatus;
    errorMessage: string;
    email: string;
    userId: string;

    constructor(private router: Router, private accountService: AccountService, private activatedRoute: ActivatedRoute) {
        this.status = ConfirmEmailStatus.Loading;

        activatedRoute.queryParams.subscribe(params => {
            this.userId = params["userId"];
            this.code = params["code"];
        });
    }

    ngOnInit() {
        this.accountService.confirmEmail({ UserId: this.userId, Code: this.code }).then(() => {
            this.status = ConfirmEmailStatus.OperationCompleted;
        }).catch((errorResponse: HttpErrorResponse) => {
            this.status = ConfirmEmailStatus.Error;
            this.errorMessage = errorResponse.error ? errorResponse.error : errorResponse.message;
        });
    }
}