import { SelectItem } from "primeng/api";

import { IHash } from "@bbwt/interfaces";
import { IColumnMetadata, IColumnType, IFolder, ITableMetadata } from "../../dbdoc";
import { IViewBuilderHandler } from "./view-builder-handler";


export interface IViewBuilderController extends IViewBuilderHandler {
    columnTypes: IColumnType[];
    controllerLoading: boolean;
    folders: IFolder[];
    possibleFilterDataTypesOfInputTypesMap: IHash<SelectItem[]>;
    possibleFilterInputTypesOfClrTypesMap: IHash<SelectItem[]>;
    possibleQueryRulesOfClrTypesMap: IHash<SelectItem[]>;
    queryColumnsMetadataMap: IHash<IColumnMetadata>;
    queryColumnsOptions: SelectItem[];
    tablesMetadata: ITableMetadata[];

    loadFullTableMetadata(tableMetadataId: number): Promise<ITableMetadata>;
}