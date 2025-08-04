import { IQueryableTableSource } from "../model/queryable-table-sources-models";
import {
    IQueryFilter,
    IQueryFilterBinding,
    IQueryFilterSet,
    IQueryTable,
    IQueryTableColumn, IQueryTableJoin
} from "../reporting-models";


export interface IQueryBuilderHandler {
    addDuplicateQueryTable(queryTableJoin: IQueryTableJoin): Promise<void>;
    addQueryFilter(parentQueryFilterSet: IQueryFilterSet, queryFilter: IQueryFilter): Promise<void>;
    addQueryFilterSet(parentQueryFilterSet: IQueryFilterSet): Promise<void>;
    addQueryTable(tableMetadataId: number): Promise<IQueryTable>;
    addQueryTablesFromSource(sources: IQueryableTableSource[]): Promise<void>;
    addQueryTableColumn(columnMetadataId: number, parentQueryTableId?: number): Promise<void>;
    addQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<IQueryTableJoin>;
    bindFilterControlToQueryFilter(filterControlId: string, queryFilter: IQueryFilter): Promise<void>;
    deleteQueryFilter(queryFilter: IQueryFilter): Promise<void>;
    deleteQueryFilterBinding(queryFilterBinding: IQueryFilterBinding): Promise<void>;
    deleteQueryFilterSet(queryFilterSet: IQueryFilterSet): Promise<void>;
    deleteQueryTable(queryTable: IQueryTable): Promise<void>;
    deleteQueryTableColumn(queryTableColumn: IQueryTableColumn): Promise<void>;
    deleteQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<void>;
    updateMasterDetailFilterBinding(queryFilterBinding: IQueryFilterBinding): Promise<void>;
    updateQueryFilter(queryFilter: IQueryFilter): Promise<void>;
    updateQueryFilterSet(queryFilterSet: IQueryFilterSet): Promise<void>;
    updateQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<IQueryTableJoin>;
}