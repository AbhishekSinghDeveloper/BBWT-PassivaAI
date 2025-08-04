import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import {
    IFolder,
    IAddDatabaseRequest,
} from "./dbdoc-models";


@Injectable({
    providedIn: "root"
})
export class DbDocFolderService extends BaseDataService {
    readonly url = "api/db-doc/folder";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    createFolder(data: IFolder): Promise<IFolder> {
        return this.httpPost("", data);
    }

    createFolderFromDb(addDatabaseRequest: IAddDatabaseRequest): Promise<IFolder> {
        return this.httpPost("create-from-db", addDatabaseRequest,
            this.handlersFactory.getForCreate(
                "Folder",
                { successMessage: "The database schema has been imported and placed to a new created folder." })
        );
    }

    syncFolderFromDb(folderId: string): Promise<IFolder> {
        return this.httpPut(`sync-from-db/${folderId}`, null,
            this.handlersFactory.getForUpdate(
                "Folder",
                { successMessage: "Folder has been synced with the connected database." })
        );
    }    

    deleteFolder(folderId: string): Promise<IFolder[]> {
        return this.httpDelete(folderId);
    }

    getFolder(folderId: string): Promise<IFolder> {
        return this.httpGet(folderId);
    }

    getFolders(): Promise<IFolder[]> {
        return this.httpGet();
    }

    getFolderOwnerTypes(): Promise<string[]> {
        return this.httpGet("owner-types");
    }

    updateFolder(folderId: string, data: IFolder): Promise<IFolder> {
        return this.httpPut(folderId, data,
            this.handlersFactory.getForUpdate(
                "Folder",
                { successMessage: "The folder has been saved." })
        );
    }

    getDbPathMacrosAllAliases(folderId: string): Promise<any[]> {
        return this.httpGet(`db-path-macros/${folderId}`);
    }
}