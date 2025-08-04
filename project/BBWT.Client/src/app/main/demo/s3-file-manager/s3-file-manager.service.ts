import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import * as moment from "moment";

import { StorageFileData } from "@main/aws-storage";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IQueryCommand } from "@features/filter";
import { IPagedData } from "@features/grid";


@Injectable({
    providedIn: "root"
})
export class S3FileManagerService extends BaseDataService {
    readonly url = "api/demo/s3-file-manager";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    getRegions(): Promise<any> {
        return this.httpGet("regions");
    }

    getPresignedUrl(key: string): Promise<string> {
        return this.httpGet(`presigned/${key}`);
    }

    getAllFiles(): Promise<StorageFileData[]> {
        return this.httpGet("files").then((res: StorageFileData[]) => this.initializeDates(res));
    }

    getAllImages(): Promise<StorageFileData[]> {
        return this.httpGet("images").then((res: StorageFileData[]) => this.initializeDates(res));
    }

    getPage(queryCommand: IQueryCommand): Promise<IPagedData<any>> {
        return this.handle<IPagedData<any>>(
            this.http.get<IPagedData<any>>(`${this.url}/page`, { params: this.constructHttpParams(queryCommand) }),
            this.handlersFactory.getForReadByFilter());
    }


    private initializeDates(files: StorageFileData[]) {
        for (const item of files) {
            item.lastModifiedDate = item.lastModifiedDate ? moment(item.lastModifiedDate).toDate() : null;
        }
        return files;
    }
}