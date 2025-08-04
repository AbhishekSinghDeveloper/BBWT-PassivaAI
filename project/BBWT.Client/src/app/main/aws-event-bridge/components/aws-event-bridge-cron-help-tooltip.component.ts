import { Component } from "@angular/core";

import { CreateMode, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";


@Component({
    selector: "bbwt-aws-event-bridge-cron-help-tooltip",
    templateUrl: "./aws-event-bridge-cron-help-tooltip.component.html"
})
export class AwsEventBridgeCronHelpTooltipComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]> [
            { field: "minutes", header: "Minutes" },
            { field: "hours", header: "Hours" },
            { field: "dayOfMonth", header: "Day of month" },
            { field: "month", header: "Month" },
            { field: "dayOfWeek", header: "Day of week" },
            { field: "year", header: "Year" },
            { field: "meaning", header: "Meaning" }
        ],
        value: [
            {
                id: 1,
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
                minutes: "0",
                hours: "18",
                dayOfMonth: "?",
                month: "*",
                dayOfWeek: "MON-FRI",
                year: "*",
                meaning: "Run at 6:00 pm (UTC) Monday through Friday"
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
