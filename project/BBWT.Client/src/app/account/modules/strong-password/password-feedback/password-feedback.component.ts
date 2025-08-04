import { Component, Input } from "@angular/core";
import { ValidCharactersSettings } from "../valid-characters";

@Component({
    selector: "password-feedback",
    templateUrl: "./password-feedback.component.html",
    styleUrls: ["./password-feedback.component.scss"]
})
export class PasswordFeedbackComponent {
    @Input() feedback;
    @Input() validCharactersSettings: ValidCharactersSettings;

    // Settings
    minlength = 8;

    // Text
    lowercaseLabel = "One lowercase character";
    uppercaseLabel = "One uppercase character";
    numberLabel = "One number";
    specialLabel = "One special character";
    minimumLabel = "characters minimum";
    messageDone = "Great! Your password is secure.";
}