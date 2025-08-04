import { Component } from "@angular/core";
import { IGridColumn, IGridSettings, ITableSettings } from "@features/grid";

@Component({
    selector: "scheduler-cron-help-tooltip",
    templateUrl: "./scheduler-cron-help-tooltip.component.html",
    styleUrls: ["./scheduler-cron-help-tooltip.component.scss"]
})
export class SchedulerCronHelpTooltipComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "seconds", header: "Seconds" },
            { field: "minutes", header: "Minutes" },
            { field: "hours", header: "Hours" },
            { field: "dayOfMonth", header: "Day of Month" },
            { field: "month", header: "Month" },
            { field: "dayOfWeek", header: "Day of Week" },
            { field: "year", header: "Year" },
            { field: "meaning", header: "Meaning" }
        ],
        value: [
            {
                id: 1,
                seconds: "0",
                minutes: "0",
                hours: "10",
                dayOfMonth: "*",
                month: "*",
                dayOfWeek: "?",
                year: "*",
                meaning: "Run at 10:00 am (UTC) every day"
            },
            {
                id: 2,
                seconds: "0",
                minutes: "15",
                hours: "12",
                dayOfMonth: "*",
                month: "*",
                dayOfWeek: "?",
                year: "*",
                meaning: "Run at 12:15 pm (UTC) every day"
            },
            {
                id: 3,
                seconds: "0",
                minutes: "0",
                hours: "18",
                dayOfMonth: "?",
                month: "*",
                dayOfWeek: "MON-FRI",
                year: "*",
                meaning: "Run at 6:00 pm (UTC) Monday through Friday"
            },
            {
                id: 4,
                seconds: "0",
                minutes: "0",
                hours: "0",
                dayOfMonth: "1",
                month: "1",
                dayOfWeek: "?",
                year: "*",
                meaning: "Run at midnight on the first day of January every year"
            }
        ],
        lazy: false,
        paginator: false
    };
    gridSettings: IGridSettings = {
        readonly: true,
        sortEnabled: false
    };
}
