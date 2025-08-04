import { Component } from "@angular/core";

import { Message } from "@bbwt/classes";
import { MessageService } from "@main/admin/services";
import { ClearTablesResult } from "../dto";
import { AwsEventBridgeTechService } from "../services";


@Component({
    selector: "bbwt-aws-event-bridge-tech",
    template: `
        <button pButton
                type="button"
                label="Clear tables"
                icon="pi pi-times"
                class="p-button-danger"
                [disabled]="loading"
                (click)="clearTables()">
        </button>

        <p-progressSpinner *ngIf="loading"
                           [style]="{ width: '25px', height: '25px' }"
                           animationDuration=".5s">
        </p-progressSpinner>

        <ng-container *ngIf="clearResult">
            <h4>Clear tables result:</h4>
            <div class="inputs-group no-labels-padding">
                <div class="inputs-group-row">
                    <div class="inputs-group-row-name">Jobs deleted:</div>
                    <div class="inputs-group-row-value">{{ clearResult.jobsDeleted }}</div>
                </div>
                <div class="inputs-group-row">
                    <div class="inputs-group-row-name">History entries deleted:</div>
                    <div class="inputs-group-row-value">{{ clearResult.historyDeleted }}</div>
                </div>
                <div class="inputs-group-row">
                    <div class="inputs-group-row-name">Running-jobs deleted:</div>
                    <div class="inputs-group-row-value">{{ clearResult.runningDeleted }}</div>
                </div>
            </div>
        </ng-container>
    `
})
export class AwsEventBridgeTechComponent {
    loading = false;
    clearResult: ClearTablesResult = null;

    constructor(private techService: AwsEventBridgeTechService,
                private messageService: MessageService) {}

    clearTables() {
        this.loading = true;
        this.techService.clearTables().then((result) => {
            this.loading = false;
            this.clearResult = result;
            this.messageService.add(Message.Success("Tables cleared!"));
        });
    }
}
