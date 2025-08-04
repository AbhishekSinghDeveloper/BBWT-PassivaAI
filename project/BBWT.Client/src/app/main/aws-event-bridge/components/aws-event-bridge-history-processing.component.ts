import { Component } from "@angular/core";

import { MessageService } from "@main/admin/services";
import { Message } from "@bbwt/classes";
import { FilterInputType, FilterType } from "@features/filter";
import {
    CreateMode,
    DisplayMode,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { AwsEventBridgeRunningJobService } from "../services";
import { displayUtc } from "../aws-event-bridge.utils";
import { AwsEventBridgeRunningJob } from "../dto";


@Component({
    selector: "bbwt-aws-event-bridge-history-processing",
    template: `
        <grid [tableSettings]="tableSettings" [gridSettings]="gridSettings"></grid>
    `
})
export class AwsEventBridgeHistoryProcessingComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
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
                displayMode: DisplayMode.Date,
                displayHandler: displayUtc,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            }
        ]
    };
    gridSettings: IGridSettings = {
        readonly: true,
        additionalRowActions: [
            {
                label: "Cancel",
                materialIcon: "cancel",
                handler: this.cancelJob.bind(this)
            }
        ],
        filtersRow: true
    };


    constructor(private runningJobService: AwsEventBridgeRunningJobService,
                private messageService: MessageService) {
        this.gridSettings.dataService = runningJobService;
    }


    cancelJob(job: AwsEventBridgeRunningJob): void {
        this.runningJobService
            .cancelJob(job.cancelationId)
            .then(() =>
                this.messageService.add(Message.Success("Cancellation request succeed."))
            );
    }
}
