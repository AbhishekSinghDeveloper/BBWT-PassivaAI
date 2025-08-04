import { Component, ViewChild } from "@angular/core";

import { Message } from "@bbwt/classes";
import { ConfirmationService, MessageService } from "@main/admin/services";
import {
    CellEditInputType,
    DisplayMode,
    GridComponent,
    GridColumnViewSettings,
    IGridColumn,
    IGridSettings,
    ITableSettings
} from "@features/grid";
import { AwsEventBridgeJobService, AwsEventBridgeRuleService } from "../services";
import { AwsEventBridgeJobInfo } from "../dto/aws-event-bridge-job-info";
import { displayUtc } from "../aws-event-bridge.utils";
import { AwsEventBridgeRule } from "../dto";

@Component({
    selector: "aws-event-bridge-rule",
    templateUrl: "./aws-event-bridge-rule.component.html"
})
export class AwsEventBridgeRuleComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name",
                editableOnUpdate: false,
                viewSettings: new GridColumnViewSettings({
                    width: "25%"
                })
            },
            {
                field: "targetJobId",
                header: "Target",
                editableOnUpdate: false,
                filterCellEnabled: false
            },
            {
                field: "lastExecutionTime",
                header: "Last Execution",
                editable: false,
                displayMode: DisplayMode.Date,
                displayHandler: displayUtc,
                filterCellEnabled: false
            },
            {
                field: "nextExecutionTime",
                header: "Next Execution",
                editable: false,
                displayMode: DisplayMode.Date,
                displayHandler: displayUtc,
                filterCellEnabled: false
            },
            {
                field: "timeZoneId",
                header: "Time zone",
                editable: false,
                filterCellEnabled: false
            },
            {
                field: "cron",
                header: "Cron Timer",
                filterCellEnabled: false,
                sortable: false
            },
            {
                field: "isEnabled",
                header: "Is Enabled",
                cellEditingInputType: CellEditInputType.Checkbox,
                displayMode: DisplayMode.Conditional,
                filterCellEnabled: false,
                sortable: false
            }
        ]
    };
    gridSettings: IGridSettings = {
        createFunc: () => {
            if (this.jobsInfo.filter(j => j.available).length) {
                this.createDialogVisible = true;
            } else {
                this.messageService.add(Message.Warning("No jobs available."));
            }
        },
        updateFunc: (rule: AwsEventBridgeRule) => {
            if (this.isAppJob(rule.targetJobId)) {
                this.editingRule = { ...rule };
                this.editDialogVisible = true;
            }
        },
        deleteFunc: (rule: AwsEventBridgeRule) => {
            if (this.isAppJob(rule.targetJobId)) {
                this.confirmationService.confirm({
                    message: `Are you sure that you want to delete job "${rule.name}"?`,
                    accept: () =>
                        this.awsEventBridgeRuleService.delete(rule.id).then(() => {
                            this.loadJobs();
                            this.eventBridgeGrid.reload();
                        })
                });
            }
        },
        filtersRow: true
    };
    createDialogVisible = false;
    editDialogVisible = false;
    editingRule: AwsEventBridgeRule = null;
    jobsInfo: AwsEventBridgeJobInfo[] = [];

    @ViewChild("eventBridgeRuleGrid", { static: true }) private eventBridgeGrid: GridComponent;

    constructor(
        private awsEventBridgeRuleService: AwsEventBridgeRuleService,
        private awsEventBridgeJobService: AwsEventBridgeJobService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.gridSettings.dataService = awsEventBridgeRuleService;
        this.loadJobs();
    }

    loadJobs(): void {
        this.awsEventBridgeJobService.getJobs().then(jobs => {
            this.jobsInfo = jobs || [];
        });
    }

    updateRow(rule: AwsEventBridgeRule): void {
        this.awsEventBridgeRuleService.update(rule.id, rule).then(() => {
            this.editDialogVisible = false;
            this.eventBridgeGrid.reload();
        });
    }

    createEBRule(rule: AwsEventBridgeRule): void {
        this.awsEventBridgeRuleService.create(rule).then(() => {
            this.createDialogVisible = false;
            this.eventBridgeGrid.reload();
            this.loadJobs();
        });
    }

    private isAppJob(targetJobId?: string) {
        const matchingJobs = this.jobsInfo.filter(j => j.jobId === targetJobId);
        if (!matchingJobs.length) {
            this.messageService.add(Message.Warning("This job seems to be external to the App."));
            return false;
        }

        return true;
    }
}
