import { Component, Input, OnChanges, SimpleChanges } from "@angular/core";

@Component({
    selector: "password-score",
    templateUrl: "./password-score.component.html",
    styleUrls: ["./password-score.component.scss"]
})
export class PasswordScoreComponent implements OnChanges {
    strength = 0;
    color: string;
    @Input() password = "";

    ngOnChanges(changes: SimpleChanges): void {
        const change = changes["password"];
        if (change) {
            this.getStrength(change.currentValue);
        }
    }

    getStrength(password) {
        if (window["zxcvbn"]) {
            const estimation = window["zxcvbn"](password || "");
            this.strength = estimation.score / 4 * 100;
            switch (estimation.score) {
                case 0:
                    this.color = "#FF0000";
                    break;
                case 1:
                    this.color = "#FF410B";
                    break;
                case 2:
                    this.color = "#FF7E00";
                    break;
                case 3:
                    this.color = "#C7FF00";
                    break;
                case 4:
                    this.color = "#00FF1B";
                    break;
            }
        }
    }
}