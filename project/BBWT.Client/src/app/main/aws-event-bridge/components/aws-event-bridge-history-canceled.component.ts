import { Component } from "@angular/core";

import * as moment from "moment";

import { IGridColumn } from "@features/grid";
import { FilterInputType, FilterType } from "@features/filter";
import { AwsEventBridgeCanceledJobService } from "../services";
import { JobCompletionStatus } from "../dto";
import { displayDuration, displayUtc } from "../aws-event-bridge.utils";


@Component({
    selector: "bbwt-aws-event-bridge-history-canceled",
    template: `
        <bbwt-aws-event-bridge-history-tab [gridColumns]="canceledJobsGridColumns"
                                           [tabJobService]="canceledJobService">
        </bbwt-aws-event-bridge-history-tab>
    `
})
export class AwsEventBridgeHistoryCanceledComponent {
    canceledJobsGridColumns: IGridColumn[] = [
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
            field: "completionStatus",
            header: "Canceled by user",
            filterCellEnabled: false,
            sortable: false,
            displayHandler: (status: JobCompletionStatus) =>
                status === JobCompletionStatus.CanceledByUser ? "Yes" : "No"
        }
    ];

    constructor(public canceledJobService: AwsEventBridgeCanceledJobService) {}
}
