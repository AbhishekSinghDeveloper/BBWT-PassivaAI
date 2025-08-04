import { Component } from "@angular/core";
import * as moment from "moment";

@Component({
    selector: "disabled",
    templateUrl: "./disabled.component.html"
})
export class DisabledComponent {
    calendarDate: Date;

    formatDate(date: Date, format: string) {
        return moment(date).format(format);
    }
}