import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IQueryCommand } from "@features/filter";
import {
    ITableData,
    ISaveTableEntityRequest,
    IDeleteTableEntityRequest,
    ITableDataViewSettings
} from "./dbdoc-models";
import { IEntity } from "@bbwt/interfaces";


@Injectable({
    providedIn: "root"
})
export class DbDocTableDataService extends BaseDataService {
    readonly url = "api/db-doc/table-data";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    GetTableDataViewSettings(): Promise<ITableDataViewSettings> {
        return this.httpGet("table-data-view-settings");
    }

    getTableData(uniqueTableId: string, folderId: string, command?: IQueryCommand): Promise<ITableData> {
        return this.handle(
            this.http.get<ITableData>(`${this.url}/table/${uniqueTableId}/${folderId}`,
                { params: this.constructHttpParams(command) }),
        );
    }

    saveTableEntity(saveTableEntityRequest: ISaveTableEntityRequest): Promise<IEntity> {
        return this.httpPost("save-table-entity", saveTableEntityRequest,
            this.handlersFactory.getForUpdate(
                "Table",
                { successMessage: "The table has been saved." }
            )
        );
    }

    deleteTableEntity(deleteTableEntityRequest: IDeleteTableEntityRequest): Promise<void> {
        return this.httpPost("delete-table-entity", deleteTableEntityRequest);
    }
}