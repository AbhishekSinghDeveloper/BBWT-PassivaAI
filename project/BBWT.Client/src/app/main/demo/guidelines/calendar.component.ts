import { Component } from "@angular/core";
import * as moment from "moment";

@Component({
    selector: "calendar",
    templateUrl: "./calendar.component.html"
})
export class CalendarComponent {
    calendarDate: Date;
    onlyDate: Date;
    onlyTime: Date;
    calendarDate2: string;
    DateTimeRex = /[0-3][0-9]\/[0-1][0-9]\/[0-9]{4} [0-1][0-9]:[0-5][0-9]/g;
    TimeRex = /[0-1][0-9]:[0-5][0-9]/g;
    DateRex = /[0-3][0-9]\/[0-1][0-9]\/[0-9]{4}/g;

    constructor() { }

    formatDate(date: Date, format: string) {
        return moment(date).format(format);
    }

    dateTimeChange(event) {
        const passed: boolean = this.DateTimeRex.test(event.target.value);
        const calendar = document.getElementById("calendar");
        const calendarDate = document.getElementById("calendarDate");
        if (passed) {
            calendar.querySelector("input").classList.remove("invalid");
            calendarDate.querySelector("input").classList.remove("invalid");
        } else {
            if (!(calendar.classList.contains("invalid"))) {
                calendar.querySelector("input").classList.add("invalid");
            }
            if (!(calendarDate.classList.contains("invalid"))) {
                calendarDate.querySelector("input").classList.add("invalid");
            }
        }
    }

    dateChange(event) {
        const passed: boolean = this.DateRex.test(event.target.value);
        const onlyDate = document.getElementById("onlyDate");
        if (passed) {
            onlyDate.querySelector("input").classList.remove("invalid");
        } else {
            if (!(onlyDate.classList.contains("invalid"))) {
                onlyDate.querySelector("input").classList.add("invalid");
            }
        }
    }

    timeChange(event) {
        const passed: boolean = this.TimeRex.test(event.target.value);
        const timeCalendar = document.getElementById("timeCalendar");
        if (passed) {
            timeCalendar.querySelector("input").classList.remove("invalid");
        } else {
            if (!(timeCalendar.classList.contains("invalid"))) {
                timeCalendar.querySelector("input").classList.add("invalid");
            }
        }
    }
}