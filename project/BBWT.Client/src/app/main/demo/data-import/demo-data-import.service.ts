import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MessageService } from "primeng/api";

import { BaseDataImportService } from "@main/data-import";
import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";

@Injectable({
    providedIn: "root"
})
export class DemoDataImportService extends BaseDataImportService {
    readonly url = "api/demo/data-import";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, messageService: MessageService) {
        super(http, handlersFactory, messageService);
    }

    getCsvFileSample(fileName: string): Promise<any> {    
        return this.handle(this.http.get(`${this.url}/csv-sample/${fileName}`, { responseType: "text" }));
    }
}