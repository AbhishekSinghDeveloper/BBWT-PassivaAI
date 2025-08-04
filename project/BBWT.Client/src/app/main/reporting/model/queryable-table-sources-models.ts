import { IColumnStaticData, ITableStaticData } from "../../dbdoc/dbdoc-models";

export interface IQueryableTableSource {
    sourceCode: string;
    sourceName: string;
    tables: IQueryableTable[];
}

export interface IQueryableTable {
    friendlyName: string;
    sourceTableAlias: string;
    schemaTable: ITableStaticData;
    schemaColumns: IColumnStaticData[];
}