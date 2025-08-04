import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { MatchPasswordValidatorDirective } from "./match-password.directive";
import { PasswordFeedbackComponent } from "./password-feedback/password-feedback.component";
import { PasswordMatchComponent } from "./password-match/password-match.component";
import { PasswordScoreComponent } from "./password-score/password-score.component";
import { StrengthMeterComponent } from "./strength-meter/strength-meter.component";
import { StrongPasswordValidatorDirective } from "./strong-password.directive";

@NgModule({
    imports: [
        CommonModule
    ],
    declarations: [
        StrongPasswordValidatorDirective,
        MatchPasswordValidatorDirective,
        StrengthMeterComponent,
        PasswordFeedbackComponent,
        PasswordScoreComponent,
        PasswordMatchComponent
    ],
    exports: [
        StrongPasswordValidatorDirective,
        MatchPasswordValidatorDirective,
        StrengthMeterComponent,
        PasswordFeedbackComponent,
        PasswordScoreComponent,
        PasswordMatchComponent
    ]
})
export class StrongPasswordModule {}