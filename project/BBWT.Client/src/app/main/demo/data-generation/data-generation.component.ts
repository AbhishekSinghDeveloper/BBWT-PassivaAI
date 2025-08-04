import { Component, EventEmitter, Input, Output } from "@angular/core";

import * as signalR from "@microsoft/signalr";


@Component({
    selector: "data-generation",
    templateUrl: "./data-generation.component.html"
})
export class DataGenerationComponent {
    generationDialogVisible = false;
    percentProgress = -1;
    recordsCountToGenerate = 50;

    @Input() linkText = "generate data";
    @Input() dialogTitle = "Generate New Records";
    @Input() connectionUrl = "api/random-data";
    @Input() startGenerationMethodName: string;
    @Input() stopGenerationMethodName = "StopGeneration";
    @Input() updateGenerationMethodName = "Update";
    @Input() generationCompletedMethodName = "Result";
    @Input() recordsLimit = 500;

    @Output() generationCompleted = new EventEmitter<void>();

    private _hubConnection: signalR.HubConnection;


    constructor() {
        this._hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(this.connectionUrl)
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        this._hubConnection.on(this.updateGenerationMethodName, (data: number) => {
            this.percentProgress = data;
        });

        this._hubConnection.on(this.generationCompletedMethodName, () => {
            this.generationDialogVisible = false;
            this.percentProgress = -1;
            this.generationCompleted.emit();
        });

        this._hubConnection
            .start()
            .catch((response: signalR.HttpError) => {
                console.error(response);
            });
    }


    get isInProgress(): boolean {
        return this.percentProgress >= 0 && this.percentProgress <= 99;
    }


    showGenerationDialog(): void {
        this.generationDialogVisible = true;
    }

    startGeneration(): void {
        this._hubConnection.invoke(this.startGenerationMethodName, this.recordsCountToGenerate)
            .then(() => this.percentProgress = 0);
    }

    stopGeneration(): void {
        this._hubConnection.invoke(this.stopGenerationMethodName)
            .then(() => this.percentProgress = -1);
    }
}