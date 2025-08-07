import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { BaseDataService, HttpResponsesHandlersFactory } from '@bbwt/modules/data-service';

export interface IAutoQueryRequest {
    tableSetId: string;
    description: string;
    maxTables?: number;
    includeRelatedTables?: boolean;
}

export interface IAutoQueryResponse {
    queryId: string;
    sql: string;
    tables: string[];
    confidence: number;
    suggestions?: string[];
}

export interface IQueryValidationRequest {
    sql: string;
    tableSetId: string;
}

export interface IQueryValidationResponse {
    isValid: boolean;
    errors: string[];
    warnings: string[];
}

@Injectable({
    providedIn: 'root'
})
export class AutoQueryBuilderService extends BaseDataService {
    readonly url = 'api/reporting3/query/auto';

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    generateQuery(request: IAutoQueryRequest): Promise<IAutoQueryResponse> {
        return this.httpPost('generate', request);
    }

    validateQuery(request: IQueryValidationRequest): Promise<IQueryValidationResponse> {
        return this.httpPost('validate', request);
    }

    getSuggestions(tableSetId: string, partialDescription: string): Promise<string[]> {
        return this.httpGet(`suggestions/${tableSetId}`, { q: partialDescription });
    }

    getAvailableTables(tableSetId: string): Promise<string[]> {
        return this.httpGet(`tables/${tableSetId}`);
    }
}