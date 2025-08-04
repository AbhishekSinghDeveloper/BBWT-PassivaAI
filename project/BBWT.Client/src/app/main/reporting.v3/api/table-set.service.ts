import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {ITableSet, ITableSetColumn, ITableSetFolder, ITableSetTable} from "@main/reporting.v3/core/reporting-models";

@Injectable({
    providedIn: "root"
})
export class TableSetService extends BaseDataService {
    readonly url = "api/reporting3/query/table-set";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    get(id: string): Promise<ITableSet> {
        return this.httpGet(`${id}`);
    }

    create(tableSet: ITableSet): Promise<ITableSet> {
        return this.httpPost(null, tableSet);
    }

    update(id: string, tableSet: ITableSet): Promise<ITableSet> {
        return this.httpPut(`${id}`, tableSet);
    }

    getFolders(): Promise<ITableSetFolder[]> {
        return this.httpGet("folders");
    }

    getFolderTables(sourceCode: string, folderId: string): Promise<ITableSetTable[]> {
        return this.httpGet(`${sourceCode}/${folderId}/tables`);
    }

    getTable(sourceCode: string, folderId: string, tableId: string, parentTableId?: string): Promise<ITableSetTable> {
        return this.handle(this.http.get<ITableSetTable>(`${this.url}/${sourceCode}/${folderId}/${tableId}/table`, {
            params: this.constructHttpParams(!!parentTableId ? {parentTableId: parentTableId} : null)
        }));
    }

    getTableColumns(sourceCode: string, folderId: string, tableId: string, parentTableId?: string): Promise<ITableSetColumn[]> {
        return this.handle(this.http.get<ITableSetColumn[]>(`${this.url}/${sourceCode}/${folderId}/${tableId}/columns`, {
            params: this.constructHttpParams(!!parentTableId ? {parentTableId: parentTableId} : null)
        }));
    }
}