import { HttpClient, HttpEvent, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { Observable } from "rxjs";
import * as moment from "moment";

import { FileDetails } from "@main/file-storage";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";

@Injectable({
    providedIn: "root"
})
export class S3Service extends BaseDataService {
    readonly url = "api/file-attachments";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    uploadFileWithProgress(formData: FormData): Observable<HttpEvent<any>> {
        const request = new HttpRequest(
            "POST", `${this.url}`, formData, { reportProgress: true }
        );
        return this.http.request(request);
    }

    async uploadFile(formData: FormData): Promise<any> {
        return await this.handleRequest<any>(
            this.http.post<any>(`${this.url}/file`, formData),
            this.handlersFactory.getDefault());
    }

    async downloadFile(key: string): Promise<any> {
        return await this.handleRequest<any>(
            this.http.get<any>(`${this.url}/${key}`),
            this.handlersFactory.getDefault());
    }

    deleteFile(key: string): Promise<boolean> {
        return this.httpDelete(`?key=${key}`);
    }

    deleteMultipleFiles(keys: string[]): Promise<boolean> {
        return this.handleRequest<any>(
            this.http.post<string[]>(`${this.url}/delete`, keys),
            this.handlersFactory.getDefault());
    }

    async uploadFileToS3(formData: FormData): Promise<any> {
        return await this.handleRequest<any>(
            this.http.post<any>(`${this.url}`, formData),
            this.handlersFactory.getDefault());
    }

    getImage(key: string): Promise<FileDetails> {
        return this.httpGet(`image/${key}`);
    }

    getAllFiles(): Promise<FileDetails[]> {
        return this.httpGet("files").then((res: FileDetails[]) => this.initializeDates(res));
    }

    getAllImages(): Promise<FileDetails[]> {
        return this.httpGet("images").then((res: FileDetails[]) => this.initializeDates(res));
    }

    getFileAttachments(formDataId: string): Promise<Blob> {
        return this.handleRequest(this.http.get(`${this.url}/attachments/${formDataId}`, { responseType: "blob" })); 
    }


    private initializeDates(files: FileDetails[]) {
        for (const item of files) {
            item.lastUpdated = item.lastUpdated ? moment(item.lastUpdated).toDate() : null;
            item.uploadTime = item.uploadTime ? moment(item.uploadTime).toDate() : null;
        }
        return files;
    }
    
}