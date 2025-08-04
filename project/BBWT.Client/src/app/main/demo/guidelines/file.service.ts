import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { CrudService, HttpResponsesHandlersFactory, IHttpResponseHandlerSettings } from "@bbwt/modules/data-service";
import { File } from "./file";
import { IFilterInfoBase, IQueryCommand } from "@features/filter";


@Injectable({
    providedIn: "root"
})
export class FileService extends CrudService<File> {
    readonly url = "api/demo/file";
    readonly entityTitle = "File";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    // Should be in the base class as getAll(filters: IFilterInfoBase[], responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<File[]> !!!
    getAllFiltered(filters: IFilterInfoBase[], responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<File[]> {
        return this.handle<File[]>(
            this.http.get<File[]>(`${this.url}`, { params: this.constructHttpParams(<IQueryCommand>{ filters: filters }) }),
            this.handlersFactory.getForReadAll(responseHandlerSettings)
        );
    }


}