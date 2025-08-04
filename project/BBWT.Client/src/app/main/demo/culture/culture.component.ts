import { Component, OnInit } from "@angular/core";
import { MessageService } from "primeng/api";
import { Message } from "@bbwt/classes";
import { CultureService } from "./culture.service";
import * as moment from "moment";

@Component({
    selector: "culture",
    templateUrl: "./culture.component.html",
    styleUrls: ["./culture.component.scss"]
})
export class CultureComponent implements OnInit {
    calendarDate: Date;
    returnedDate: Date;
    cultures: any[];
    selectedCulture: any;

    constructor(private cultureService: CultureService, private messageService: MessageService) { }

    ngOnInit() {
        this.calendarDate = moment().toDate();
        this.cultures = [];
        this.cultures.push({ label: "English (UK)", value: "en-gb" });
        this.cultures.push({ label: "English (US)", value: "en" });
        this.cultures.push({ label: "Arabic", value: "ar" });
        this.cultures.push({ label: "German", value: "de" });
        this.cultures.push({ label: "Russian", value: "ru" });
        this.cultures.push({ label: "Japanese", value: "ja" });
        this.cultures.push({ label: "French", value: "fr" });
    }

    getMomentDate(formatStr: string) {
        moment.locale(this.selectedCulture);
        return moment(this.calendarDate).format(formatStr);
    }

    getTimeZone = () => {
        return moment().format("Z");
    }

    onPostDate() {
        this.cultureService.getCulture(this.calendarDate).then((data: any) => {
            this.messageService.add(Message.Success("The date " + data.serverDate + " has been posted successfully."));
            this.returnedDate = new Date(data.serverDate);
        }).catch((err: any) => {
            this.messageService.add(Message.Error("Error while posting date to server."));
        });
    }
}