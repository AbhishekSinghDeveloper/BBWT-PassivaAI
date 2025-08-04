import { Component } from "@angular/core";

import * as moment from "moment";

import { FilterInputType, FilterType } from "@features/filter";
import { GridColumnViewSettings, IGridColumn } from "@features/grid";
import { AwsEventBridgeFailedJobService } from "../services/aws-event-bridge-failed-job.service";
import { displayDuration, displayUtc } from "../aws-event-bridge.utils";


@Component({
    selector: "bbwt-aws-event-bridge-history-failed",
    template: `
        <bbwt-aws-event-bridge-history-tab [gridColumns]="failedJobsGridColumns"
                                           [tabJobService]="failedJobService">
        </bbwt-aws-event-bridge-history-tab>
    `
})
export class AwsEventBridgeHistoryFailedComponent {
    failedJobsGridColumns: IGridColumn[] = [
        {
            field: "id",
            header: "Id"
        },
        {
            field: "ruleId",
            header: "Name"
        },
        {
            field: "startTime",
            header: "Start Time",
            displayHandler: displayUtc,
            filterSettings: {
                filterType: FilterType.Date,
                inputType: FilterInputType.Calendar
            }
        },
        {
            field: "finishTime",
            header: "Duration",
            filterCellEnabled: false,
            displayHandler: (value: moment.Moment, data: { startTime: moment.Moment }) =>
                displayDuration(data.startTime, value)
        },
        {
            field: "errorMessage",
            header: "Error",
            filterCellEnabled: false,
            viewSettings: new GridColumnViewSettings({ width: "15%" })
        },
        {
            field: "stackTrace",
            header: "Stack",
            filterCellEnabled: false,
            sortable: false,
            viewSettings: new GridColumnViewSettings({ width: "15%" })
        }
    ];

    constructor(public failedJobService: AwsEventBridgeFailedJobService) {}
}
