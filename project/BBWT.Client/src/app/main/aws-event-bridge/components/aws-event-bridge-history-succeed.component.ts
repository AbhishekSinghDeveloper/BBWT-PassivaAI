import { Component } from "@angular/core";

import { FilterInputType, FilterType } from "@features/filter";
import { IGridColumn } from "@features/grid";
import { AwsEventBridgeSucceedJobService } from "../services";
import { displayDuration, displayUtc } from "../aws-event-bridge.utils";


@Component({
    selector: "bbwt-aws-event-bridge-history-succeed",
    template: `
        <bbwt-aws-event-bridge-history-tab [gridColumns]="succeedJobsGridColumns"
                                           [tabJobService]="succeedJobService">
        </bbwt-aws-event-bridge-history-tab>`
})
export class AwsEventBridgeHistorySucceedComponent {
    succeedJobsGridColumns: IGridColumn[] = [
        {
            field: "id",
            header: "Id"
        },
        {
            field: "ruleId",
            header: "Name"
        },
        {
            field: "jobId",
            header: "Target",
            filterCellEnabled: false
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
            displayHandler: (value: moment.Moment, data: { startTime: moment.Moment }) =>
                displayDuration(data.startTime, value),
            filterCellEnabled: false
        }
    ];

    constructor(public succeedJobService: AwsEventBridgeSucceedJobService) {}
}
