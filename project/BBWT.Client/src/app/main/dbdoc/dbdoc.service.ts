import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import {
    ICopyTableMetadataToFolderRequest,
    IColumnMetadata,
    IFolder,
    ITableMetadata,
    IColumnValidationMetadata,
    IColumnViewMetadata,
    ITableMetadataResult,
    ISetColumnTypeMetadataForColumnMetadataRequest
} from "./dbdoc-models";


@Injectable({
    providedIn: "root"
})
export class DbDocService extends BaseDataService {
    readonly url = "api/db-doc";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    copyTableMetadataToFolder(data: ICopyTableMetadataToFolderRequest): Promise<IFolder> {
        return this.httpPost("copy-table-metadata-to-folder", data,
            this.handlersFactory.getForUpdate(
                "Table",
                { successMessage: "The table has been copied to folder" }
            ));
    }

    deleteColumnValidationMetadata(columnMetadataId: number): Promise<void> {
        return this.httpPost(`delete-column-validation-metadata/${columnMetadataId}`);
    }

    deleteColumnViewMetadata(columnMetadataId: number): Promise<void> {
        return this.httpPost(`delete-column-view-metadata/${columnMetadataId}`);
    }

    deleteTableMetadata(tableMetadataId: number): Promise<IFolder> {
        return this.httpDelete(`delete-table-metadata/${tableMetadataId}`);
    }

    getTableMetadata(uniqueTableId: string, folderId?: string): Promise<ITableMetadataResult> {
        return this.httpGet(`get-table-metadata/${uniqueTableId}${folderId ? "/" + folderId : ""}`)
            .then(result => {
                const returnResult = <ITableMetadataResult>{};
                Object.keys(result).forEach(key => returnResult[key] = result[key]);
                return returnResult;
            });
    }

    setColumnTypeMetadataForColumnMetadata(data: ISetColumnTypeMetadataForColumnMetadataRequest): Promise<IColumnMetadata> {
        return this.httpPost("set-column-type-metadata-for-column-metadata", data);
    }

    setColumnValidationMetadata(columnMetadataId: number, data: IColumnValidationMetadata): Promise<IColumnValidationMetadata> {
        return this.httpPost(`set-column-validation-metadata/${columnMetadataId}`, data);
    }

    setColumnViewMetadata(columnMetadataId: number, data: IColumnViewMetadata): Promise<IColumnViewMetadata> {
        return this.httpPost(`set-column-view-metadata/${columnMetadataId}`, data);
    }

    updateColumnMetadata(columnMetadataId: number, data: IColumnMetadata, toastNotify: boolean = true): Promise<IColumnMetadata> {
        return this.httpPut(`update-column-metadata/${columnMetadataId}`, data,
            toastNotify ?
                this.handlersFactory.getForUpdate(
                    "Column",
                    { successMessage: "The column has been saved." })
                : this.handlersFactory.getDefault()
        );
    }

    updateTableMetadata(tableMetadataId: number, data: ITableMetadata): Promise<ITableMetadata> {
        return this.httpPut(`update-table-metadata/${tableMetadataId}`, data,
            this.handlersFactory.getForUpdate(
                "Table",
                { successMessage: "The table has been saved." })
        );
    }

    exportAnonymizationXml(folderId: string): Promise<any> {
        return this.handle(this.http.get(`${this.url}/anonymization-xml/${folderId}`, { responseType: "text" }));
    }
}