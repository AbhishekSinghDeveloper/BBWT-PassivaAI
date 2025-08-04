import { Component, Input } from "@angular/core";

import { Message } from "@bbwt/classes";
import { PagedReaderService } from "@features/grid";
import { MessageService } from "@main/admin/services";
import { FilterInputType, FilterType } from "@features/filter";
import {
    CreateMode,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { AwsEventBridgeJobHistory } from "../dto";
import { AwsEventBridgeJobService } from "../services";
import { displayUtc } from "@main/aws-event-bridge/aws-event-bridge.utils";


@Component({
    selector: "bbwt-aws-event-bridge-history-tab",
    template: `
        <grid [tableSettings]="tableSettings" [gridSettings]="gridSettings"></grid>
    `
})
export class AwsEventBridgeHistoryTabComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "id",
                header: "ID"
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
            }
        ]
    };
    gridSettings: IGridSettings = {
        additionalRowActions: [
            {
                label: "Restart",
                materialIcon: "replay",
                handler: this.restartJob.bind(this)
            }
        ],
        readonly: true,
        filtersRow: true
    };


    constructor(private jobService: AwsEventBridgeJobService, private messageService: MessageService) {}


    @Input() set gridColumns(value: IGridColumn[]) {
        if (!value) return;
        this.tableSettings.columns = value;
    }

    @Input() set additionalRowActions(value: IGridActionsRowButton[]) {
        if (!value) return;
        this.gridSettings.additionalRowActions = value;
    }

    @Input() set tabJobService(value: PagedReaderService<AwsEventBridgeJobHistory>) {
        if (!value) return;
        this.gridSettings.dataService = value;
    }


    restartJob(row: AwsEventBridgeJobHistory): void {
        const historyId = row.id_original || row.id;
        this.jobService
            .restartJob(historyId)
            .then(() => this.messageService.add(Message.Success("Job restart successful.")));
    }
}
