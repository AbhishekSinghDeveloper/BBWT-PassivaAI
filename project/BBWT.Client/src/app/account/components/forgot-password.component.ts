import { HttpErrorResponse } from "@angular/common/http";
import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { MessageService } from "primeng/api";
import { Message } from "@bbwt/classes";
import { AccountService } from "../services";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { RecoverPassword } from "../interfaces";

@Component({
    selector: "forgot-password",
    templateUrl: "./forgot-password.component.html"
})
export class ForgotPasswordComponent {
    email: string;

    get _ValidationPatterns(): any {
        return ValidationPatterns;
    }

    constructor(
        private router: Router,
        private messageService: MessageService,
        private accountService: AccountService
    ) {}

    onSubmit(): void {
        this.accountService
            .recoverPassword({ email: this.email } as RecoverPassword)
            .then(() => {
                this.messageService.add(
                    Message.Success(
                        "If your email address was registered with us, an email has now been sent " +
                        "to your address. This email will allow you to reset your password",
                        "Recover Password"
                    )
                );
                this.router.navigate(["/account"]);
            })
            .catch((errorResponse: HttpErrorResponse) => {
                const message = errorResponse.error ? errorResponse.error : errorResponse.message;
                this.messageService.add(
                    Message.Error(
                        "An error occurred while sending email. " + message,
                        "Recover Password"
                    )
                );
            });
    }
}
