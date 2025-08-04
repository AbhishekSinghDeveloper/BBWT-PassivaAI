import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { PagedCrudService } from "@features/grid";
import {
    HttpResponsesHandlersFactory
} from "@bbwt/modules/data-service/http-responses-handler";
import { IColumnType } from "./column-type-models";
import { IColumnValidationMetadata, IColumnViewMetadata } from "../dbdoc-models";


@Injectable({
    providedIn: "root"
})
export class ColumnTypeService extends PagedCrudService<IColumnType> {
    readonly url = "api/dbdoc/column-type";
    readonly entityTitle = "Column Type";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    deleteValidationMetadata(columnTypeId: string): Promise<null> {
        return this.httpPost(`delete-validation-metadata/${columnTypeId}`, null,
            this.handlersFactory.getForUpdate(
                "Column Type",
                { successMessage: "The validation metadata has been deleted from column type." })
        );
    }

    deleteViewMetadata(columnTypeId: string): Promise<null> {
        return this.httpPost(`delete-view-metadata/${columnTypeId}`, null,
            this.handlersFactory.getForUpdate(
                "Column Type",
                { successMessage: "The view metadata has been deleted from column type." })
        );
    }

    setValidationMetadata(columnTypeId: string, validationMetadata: IColumnValidationMetadata): Promise<IColumnValidationMetadata> {
        return this.httpPost(`set-validation-metadata/${columnTypeId}`, validationMetadata,
            this.handlersFactory.getForUpdate(
                "Column Type",
                { successMessage: "The validation metadata has been set for column type." })
        );
    }

    setViewMetadata(columnTypeId: string, viewMetadata: IColumnViewMetadata): Promise<IColumnViewMetadata> {
        return this.httpPost(`set-view-metadata/${columnTypeId}`, viewMetadata,
            this.handlersFactory.getForUpdate(
                "Column Type",
                { successMessage: "The view metadata has been set for column type." })
        );
    }
}