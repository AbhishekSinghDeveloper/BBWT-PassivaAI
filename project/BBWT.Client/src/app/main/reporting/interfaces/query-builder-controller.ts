import { SelectItem } from "primeng/api";

import { IHash } from "@bbwt/interfaces";
import { IQueryRule } from "../reporting-models";
import { IColumnMetadata, IColumnType, IFolder, ITableMetadata } from "../../dbdoc";
import { IQueryFilterDataMap } from "../components/section-editor.component";
import { IQueryBuilderHandler } from "./query-builder-handler";


export interface IQueryBuilderController extends IQueryBuilderHandler {
    columnsMetadata: IColumnMetadata[];
    columnTypes: IColumnType[];
    filterControlsOptions: SelectItem[];
    filterTreeLoading: boolean;
    folders: IFolder[];
    controllerLoading: boolean;
    possibleQueryRulesOfClrTypesMap: IHash<SelectItem[]>;
    queryColumnsMetadataMap: IHash<IColumnMetadata>;
    queryColumnsOptions: SelectItem[];
    queryFiltersRelatedDataMap: IQueryFilterDataMap;
    queryRules: IQueryRule[];
    queryTablesMetadataMap: IHash<ITableMetadata>;
    queryTreeLoading: boolean;
    tablesMetadata: ITableMetadata[];

    loadFolderStructure(folderId?: string): Promise<void>;
    loadFullTableMetadata(tableMetadataId: number): Promise<ITableMetadata>;
    requestRawSql(): Promise<string>;
}