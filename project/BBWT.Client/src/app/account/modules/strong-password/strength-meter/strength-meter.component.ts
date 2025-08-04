import { Component, OnInit, Input, OnChanges, SimpleChanges, Output, EventEmitter } from "@angular/core";
import { ZxcvbnLoad } from "../zxcvbn-load";

@Component({
    selector: "strength-meter",
    templateUrl: "./strength-meter.component.html",
    styleUrls: ["./strength-meter.component.scss"]
})
export class StrengthMeterComponent implements OnInit, OnChanges {
    @Input() password = "";

    passwordStrength = 0;
    crack_times_display: {};
    private zxcvbn;

    @Output() strength = new EventEmitter();

    ngOnInit() {
        ZxcvbnLoad.load().then(zxcvbn => {
            this.zxcvbn = zxcvbn;
            this.getStrength(this.password);
        });
    }

    ngOnChanges(changes: SimpleChanges): void {
        const change = changes["password"];
        if (change) {
            this.getStrength(change.currentValue);
        }
    }

    getStrength(password) {
        if (this.zxcvbn) {
            const estimation = this.zxcvbn(password || "");
            this.passwordStrength = estimation.score;
            this.crack_times_display = estimation.crack_times_display;
            this.strength.emit({
                strength: this.passwordStrength
            });
        }
    }

    getClass() {
        return `level-${this.passwordStrength}`;
    }
}