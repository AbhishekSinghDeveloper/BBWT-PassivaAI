import { Component, Input, OnChanges, SimpleChanges } from "@angular/core";


@Component({
    selector: "password-match",
    templateUrl: "./password-match.component.html",
    styleUrls: ["./password-match.component.scss"]
})
export class PasswordMatchComponent implements OnChanges {
    confirmPasswordMessage = "";

    @Input() confirmPassword = "";
    @Input() password = "";

    ngOnChanges(changes: SimpleChanges): boolean {
        const changeConfirmPassword = changes["confirmPassword"];
        const changePassword = changes["password"];

        if (changeConfirmPassword) {
            return this.matchPassword(this.password, changeConfirmPassword.currentValue);
        }

        if (changePassword) {
            return this.matchPassword(changePassword.currentValue, this.confirmPassword);
        }
    }

    matchPassword(password: string, confirmPassword: string): boolean {
        if (confirmPassword !== password) {
            this.confirmPasswordMessage = "Passwords must match";
            return false;
        } else {
            this.confirmPasswordMessage = "";
            return true;
        }
    }
}