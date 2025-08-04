import { HttpClient } from "@angular/common/http";
import { EventEmitter } from "@angular/core";

import * as signalR from "@microsoft/signalr";
import { MessageService } from "primeng/api";

import { IGridImportService } from "@features/grid";
import { Message } from "@bbwt/classes";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { ColumnDefinition } from "./column-definition";
import { ImportConfig } from "./import-config";
import { ImportResult } from "./import-result";


export abstract class BaseDataImportService extends BaseDataService implements IGridImportService {
    private _config: ImportConfig;
    private _customEventNames: string[] = [];
    private _hubConnection: signalR.HubConnection;
    private _onImported: EventEmitter<ImportResult>;
    private _onImportStopped: EventEmitter<void>;
    private _onUpdate: EventEmitter<number>;


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, private messageService: MessageService) {
        super(http, handlersFactory);

        this._onImported = new EventEmitter<ImportResult>();
        this._onUpdate = new EventEmitter<number>();
        this._onImportStopped = new EventEmitter<void>();

        this.initHubConnection();
    }


    get columnDefinitions(): ColumnDefinition[] {
        if (this._config) {
            return this._config.columnDefinitions;
        }
        return null;
    }

    get firstRow(): number {
        return this._config.firstRow;
    }

    get importBreak() {
        return this._onImportStopped;
    }

    get importProgress() {
        return this._onUpdate;
    }

    get importSuccess() {
        return this._onImported;
    }

    get maxErrorsCount(): number {
        if (this._config) {
            return this._config.maxErrorsCount;
        }
        return null;
    }


    breakImporting(): void {
        this._hubConnection.invoke("Stop");
    }

    importFile(file: File): Promise<void> {
        if (this._hubConnection.state === signalR.HubConnectionState.Disconnected) {
            this.initHubConnection();
        }

        const formData = new FormData();
        formData.append("file", file);
        formData.append("config", JSON.stringify(this._config));

        this.messageService.add(Message.Info("Import is starting...", " "));

        return this.httpPost("import", formData,
            {
                onSuccess: () => this.messageService.add(Message.Info("Import completed", " ")),
                onError: response => {
                    this.messageService.add(Message.Error(response.message));
                    return false;
                }
            }
        );
    }

    setColumnDefinitions(value: ColumnDefinition[]) {
        if (this._config) {
            this._config.columnDefinitions = value;
        }
    }

    setConfig(value: ImportConfig) {
        this._config = value;
    }

    setCustomEvent(eventName: string, callback: (result?: any) => void): void {
        this._customEventNames.push(eventName);

        this._hubConnection.on(eventName, callback);
    }

    setFirstRow(value: number) {
        if (value >= 0 && this._config) {
            this._config.firstRow = value;
        }
    }

    setLastRow(value?: number) {
        if (this._config) {
            this._config.lastRow = value;
        }
    }

    setMaxErrorsCount(value: number) {
        if (value >= 0 && this._config) {
            this._config.maxErrorsCount = value;
        }
    }


    private initHubConnection(): void {
        this._hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("api/import-processing")
            .configureLogging(signalR.LogLevel.None)
            .build();

        this._hubConnection.start();

        this._hubConnection.on("Update", (data: number) => this._onUpdate.emit(data));
        this._hubConnection.on("Result", (result: ImportResult) => {
            this._onImported.emit(result);
            this._hubConnection.stop();
        });

        this._hubConnection.on("Stopped", () => this._onImportStopped.emit());
    }
}