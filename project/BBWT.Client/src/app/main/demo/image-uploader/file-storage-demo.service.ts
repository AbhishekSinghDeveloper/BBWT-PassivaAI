import { HttpClient, HttpEvent, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { Observable } from "rxjs";
import * as moment from "moment";

import { FileDetails } from "@main/file-storage";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";

@Injectable({
    providedIn: "root"
})
export class FileStorageDemoService extends BaseDataService {
    readonly url = "api/demo/file-storage";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    uploadFile(formData: FormData): Observable<HttpEvent<any>> {
        const request = new HttpRequest(
            "POST", `${this.url}`, formData, { reportProgress: true }
        );
        return this.http.request(request);
    }

    async uploadFileToS3(formData: FormData): Promise<any> {
        return await this.handleRequest<any>(
            this.http.post<any>(`${this.url}`, formData),
            this.handlersFactory.getDefault());
    }


    deleteFile(key: string): Promise<boolean> {
        return this.httpDelete(`?key=${key}`);
    }

    getAllFiles(): Promise<FileDetails[]> {
        return this.httpGet("files").then((res: FileDetails[]) => this.initializeDates(res));
    }

    getAllImages(): Promise<FileDetails[]> {
        return this.httpGet("images").then((res: FileDetails[]) => this.initializeDates(res));
    }


    private initializeDates(files: FileDetails[]) {
        for (const item of files) {
            item.lastUpdated = item.lastUpdated ? moment(item.lastUpdated).toDate() : null;
            item.uploadTime = item.uploadTime ? moment(item.uploadTime).toDate() : null;
        }
        return files;
    }
}