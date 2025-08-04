import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { IQueryCommand } from "@features/filter";
import {
    IFilterControl,
    IQueryFilter,
    IQueryFilterSet,
    ISection,
    IGridViewColumn,
    IGridView,
    ISectionDisplayView,
    IQuery,
    IView,
    IReportChangeResult,
    IQueryTable,
    IQueryTableColumn,
    IQueryFilterBinding,
    IQueryTableJoin
} from "../reporting-models";
import { SelectItem } from "primeng/api";
import { ITableMetadata } from "@main/dbdoc";
import { IQueryableTableSource } from "../model/queryable-table-sources-models";


@Injectable()
export class SectionService extends PagedCrudService<ISection> {
    readonly url = "api/reporting/section";
    readonly entityTitle = "Section";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    addDuplicateQueryTable(sectionId: string, queryTableJoin: IQueryTableJoin): Promise<IReportChangeResult<IQueryTableJoin>> {
        return this.httpPost(`${sectionId}/add-duplicate-query-table`, queryTableJoin,
            this.handlersFactory.getForCreate("Duplicate Query Table", {
                showSuccessMessage: false,
                showErrorMessage: false
            })
        );
    }

    addFilterControl(sectionId: string, data: IFilterControl): Promise<IReportChangeResult<IFilterControl>> {
        return this.httpPost(`${sectionId}/add-filter-control`, data,
            this.handlersFactory.getForCreate("Filter Control", { showSuccessMessage: false })
        );
    }

    addQueryFilter(sectionId: string, queryFilter: IQueryFilter): Promise<IReportChangeResult<IQueryFilter>> {
        return this.httpPost(`${sectionId}/add-query-filter`, queryFilter,
            this.handlersFactory.getForCreate("Query Filter", { showSuccessMessage: false  })
        );
    }

    addQueryFilterSet(sectionId: string, parentQueryFilterSetId: string): Promise<IReportChangeResult<IQueryFilterSet>> {
        return this.httpPost(`${sectionId}/add-query-filter-set/${parentQueryFilterSetId}`, null,
            this.handlersFactory.getForCreate("Query Filter Set", { showSuccessMessage: false })
        );
    }

    addQueryTable(sectionId: string, tableMetadataId: number): Promise<IReportChangeResult<IQueryTable>> {
        return this.httpPost(`${sectionId}/add-query-table/${tableMetadataId}`, null,
            this.handlersFactory.getForCreate("Query Table", {
                showSuccessMessage: false,
                // Avoiding showing message: user may check/uncheck multiple times in tables tree
                // which leads to request conflicts correctly handled by back- end.Then
                // we only log a fact of error, not showing for user
                showErrorMessage: false
            })
        );
    }

    addQueryTablesFromSource(sectionId: string, sources: IQueryableTableSource[]): Promise<IReportChangeResult<IQueryTable[]>> {
        return this.httpPost(`${sectionId}/add-query-tables-from-source`, sources,
            this.handlersFactory.getForCreate("Query Table", {
                showSuccessMessage: false,
                // Avoiding showing message: user may check/uncheck multiple times in tables tree
                // which leads to request conflicts correctly handled by back- end.Then
                // we only log a fact of error, not showing for user
                showErrorMessage: false
            })
        );
    }

    addQueryTableColumn(sectionId: string, columnMetadataId: number, parentQueryTableId?: number): Promise<IReportChangeResult<IQueryTableColumn>> {
        return this.httpPost(`${sectionId}/add-query-table-column/${columnMetadataId}`, parentQueryTableId,
            this.handlersFactory.getForCreate("Query Table Column", {
                showSuccessMessage: false,
                // Avoiding showing message: user may check/uncheck multiple times in tables tree
                // which leads to request conflicts correctly handled by back- end.Then
                // we only log a fact of error, not showing for user
                showErrorMessage: false
            })
        );
    }

    addQueryTableJoin(sectionId: string, queryTableJoin: IQueryTableJoin): Promise<IReportChangeResult<IQueryTableJoin>> {
        return this.httpPost(`${sectionId}/add-query-table-join`, queryTableJoin,
            this.handlersFactory.getForCreate(
                "Query Table Join",
                {
                    showSuccessMessage: false,
                    showErrorMessage: false
                })
        );
    }

    bindFilterControlToQueryFilter(sectionId: string, filterControlId: string, queryFilterId: string): Promise<IReportChangeResult<IQueryFilterBinding>> {
        return this.httpPost(`${sectionId}/bind-filter-control/${filterControlId}/to-query-filter/${queryFilterId}`, null,
            this.handlersFactory.getForCreate("Query Filter Binding", { showSuccessMessage: false  })
        );
    }

    deleteFilterControl(sectionId: string, filterControlId: string, deleteLinkedQueryFilters: boolean): Promise<IReportChangeResult> {
        return this.handle(
            this.http.delete<IReportChangeResult>(
                `${this.url}/${sectionId}/delete-filter-control/${filterControlId}`,
                { params: this.constructHttpParams({ deleteLinkedQueryFilters: deleteLinkedQueryFilters }) }),
            this.handlersFactory.getForDelete("Filter Control", { showSuccessMessage: false  })
        );
    }

    deleteQueryFilter(sectionId: string, queryFilterId: string): Promise<IReportChangeResult> {
        return this.httpDelete(`${sectionId}/delete-query-filter/${queryFilterId}`,
            this.handlersFactory.getForDelete("Query Filter", { showSuccessMessage: false  })
        );
    }

    deleteQueryFilterBinding(sectionId: string, queryFilterBindingId: string): Promise<IReportChangeResult> {
        return this.httpDelete(`${sectionId}/delete-query-filter-binding/${queryFilterBindingId}`,
            this.handlersFactory.getForDelete("Query Filter Binding", { showSuccessMessage: false  })
        );
    }

    deleteQueryFilterSet(sectionId: string, queryFilterSetId: string): Promise<IReportChangeResult> {
        return this.httpDelete(`${sectionId}/delete-query-filter-set/${queryFilterSetId}`,
            this.handlersFactory.getForDelete("Query Filter Set", { showSuccessMessage: false  })
        );
    }

    deleteQueryTable(sectionId: string, queryTableId: string): Promise<IReportChangeResult> {
        return this.httpDelete(`${sectionId}/delete-query-table/${queryTableId}`,
            this.handlersFactory.getForDelete("Query Table", {
                showSuccessMessage: false,
                // Avoiding showing message: user may check/uncheck multiple times in tables tree
                // which leads to request conflicts correctly handled by back- end.Then
                // we only log a fact of error, not showing for user
                showErrorMessage: false
            })
        );
    }

    deleteQueryTableColumn(sectionId: string, queryTableColumnId: string): Promise<IReportChangeResult> {
        return this.httpDelete(`${sectionId}/delete-query-table-column/${queryTableColumnId}`,
            this.handlersFactory.getForDelete("Query Table Column", {
                showSuccessMessage: false,
                // Avoiding showing message: user may check/uncheck multiple times in tables tree
                // which leads to request conflicts correctly handled by back- end.Then
                // we only log a fact of error, not showing for user
                showErrorMessage: false
            })
        );
    }

    deleteQueryTableJoin(sectionId: string, queryTableJoinId: string): Promise<IReportChangeResult> {
        return this.httpDelete(`${sectionId}/delete-query-table-join/${queryTableJoinId}`,
            this.handlersFactory.getForDelete(
                "Query Table Join",
                {
                    showSuccessMessage: false,
                    showErrorMessage: false
                })
        );
    }

    getAggregations(sectionId: string, queryCommand: IQueryCommand): Promise<{[key: string]: any[]}> {
        return this.handleRequest(
            this.http.get<{[key: string]: any[]}>(`${this.url}/${sectionId}/fetch-aggregations`, { params: this.constructHttpParams(queryCommand) }),
            this.handlersFactory.getDefault()
        );
    }

    getData(sectionId: string, queryCommand: IQueryCommand): Promise<any[]> {
        return this.handle(
            this.http.get<any[]>(`${this.url}/${sectionId}/fetch-data`,
                { params: this.constructHttpParams(queryCommand) })
        );
    }

    getDisplayView(sectionId: string): Promise<ISectionDisplayView> {
        return this.httpGet(`${sectionId}/display-view`);
    }

    getFilterControls(sectionId: string): Promise<IFilterControl[]> {
        return this.httpGet(`${sectionId}/filter-controls`);
    }

    getFilterOptions(sectionId: string, filterControlId: string): Promise<SelectItem[]> {
        return this.httpGet(`${sectionId}/filter-options/${filterControlId}`);
    }

    getQueryableTableSources(): Promise<any> {
        return this.httpGet("quaryable-table-sources");
    }

    getQueryStructure(sectionId: string): Promise<IQuery> {
        return this.httpGet(`${sectionId}/query-structure`);
    }

    getSectionTablesMetadata(sectionId: string): Promise<ITableMetadata[]> {
        return this.httpGet(`${sectionId}/tables-metadata`);
    }

    getReachableTables(sectionId: string): Promise<string[]> {
        return this.httpGet(`${sectionId}/reachable-tables`);
    }

    getSqlQuery(sectionId: string): Promise<{sql: string}> {
        return this.httpGet(`${sectionId}/sql-query`);
    }

    getTotal(sectionId: string, queryCommand: IQueryCommand): Promise<number> {
        return this.handle(
            this.http.get<number>(`${this.url}/${sectionId}/fetch-total`,
                { params: this.constructHttpParams(queryCommand) }),
        );
    }

    getViewSettings(sectionId: string): Promise<IView> {
        return this.httpGet(`${sectionId}/view-settings`);
    }

    toggleSectionAllGridViewColumnsSortable(sectionId: string, value: boolean): Promise<void> {
        return this.httpPost(`${sectionId}/toggle-grid-view-columns-sortable`, value);
    }

    toggleSectionAllGridViewColumnsVisible(sectionId: string, value: boolean): Promise<void> {
        return this.httpPost(`${sectionId}/toggle-grid-view-columns-visible`, value);
    }

    updateFilterControl(sectionId: string, filterControlId: string, data: IFilterControl): Promise<IReportChangeResult<IFilterControl>> {
        return this.httpPut(`${sectionId}/update-filter-control/${filterControlId}`, data,
            this.handlersFactory.getForUpdate("Filter Control", { showSuccessMessage: false  })
        );
    }

    updateMasterDetailQueryFilterBinding(sectionId: string, binding: IQueryFilterBinding): Promise<IReportChangeResult<IQueryFilterBinding>> {
        return this.httpPost(`${sectionId}/update-master-detail-query-filter-binding`, binding,
            this.handlersFactory.getForCreate("Query Filter Binding", { showSuccessMessage: false })
        );
    }

    updateQueryFilter(sectionId: string, queryFilterId: string, data: IQueryFilter): Promise<IReportChangeResult<IQueryFilter>> {
        return this.httpPut(`${sectionId}/update-query-filter/${queryFilterId}`, data,
            this.handlersFactory.getForUpdate("Query Filter", { showSuccessMessage: false  })
        );
    }

    updateSqlFilter(sectionId: string, queryFilterId: string, data: IQueryFilter): Promise<IReportChangeResult<IQueryFilter>> {
        return this.httpPut(`${sectionId}/update-sql-filter/${queryFilterId}`, data,
            this.handlersFactory.getForUpdate("SQL Filter", { showSuccessMessage: false })
        );
    }

    updateQueryFilterSet(sectionId: string, queryFilterSetId: string, data: IQueryFilterSet): Promise<IReportChangeResult<IQueryFilterSet>> {
        return this.httpPut(`${sectionId}/update-query-filter-set/${queryFilterSetId}`, data,
            this.handlersFactory.getForUpdate("Query Filter Set", { showSuccessMessage: false  })
        );
    }

    updateGridView(sectionId: string, gridViewId: string, data: IGridView): Promise<IReportChangeResult<IGridView>> {
        return this.httpPut(`${sectionId}/update-grid-view/${gridViewId}`, data,
            this.handlersFactory.getForUpdate("Grid View", { showSuccessMessage: false  })
        );
    }

    updateGridViewColumn(sectionId: string, gridViewColumnId: string, data: IGridViewColumn): Promise<IReportChangeResult<IGridViewColumn>> {
        return this.httpPut(`${sectionId}/update-grid-view-column/${gridViewColumnId}`, data,
            this.handlersFactory.getForUpdate("Grid View Column", { showSuccessMessage: false  })
        );
    }

    updateQueryTableJoin(sectionId: string, queryTableJoinId: string, queryTableJoin: IQueryTableJoin): Promise<IReportChangeResult<IQueryTableJoin>> {
        return this.httpPost(`${sectionId}/update-query-table-join/${queryTableJoinId}`, queryTableJoin,
            this.handlersFactory.getForUpdate(
                "Query Table Join",
                {
                    showSuccessMessage: false,
                    showErrorMessage: false
                })
        );
    }
}