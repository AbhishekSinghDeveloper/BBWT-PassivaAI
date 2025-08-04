import {DisplayMode, IGridColumn, SortOrder} from "@features/grid";
import {DataType, InputType, IWidgetSource} from "../core/reporting-models";
import {IQueryVariables} from "../core/variables/variable-models";


export interface IGridView {
    id: string;

    isRowSelectable: boolean;
    defaultSortOrder: SortOrder;
    summaryFooterVisible: boolean;
    defaultSortColumnAlias: string;
    showVisibleColumnsSelector: boolean;

    // Foreign keys and navigational properties.
    querySourceId: string;
    widgetSourceId: string;

    widgetSource: IWidgetSource;

    columns: IGridViewColumn[];
}

export interface IGridViewColumn {
    id: string;

    customColumnTypeId: string;
    dataType: DataType;
    displayMode: DisplayMode;
    extraSettings: any;
    footer: any;
    header: string;
    inheritHeader: boolean;
    inputType: InputType;
    queryAlias: string;
    sortOrder: number;
    sortable: boolean;
    visible: boolean;

    // Foreign keys and navigational properties.
    variableName: string;
    gridId: string;

    grid: IGridView;
}

export interface IGridDisplayView {
    id: string;

    isRowSelectable: boolean;
    summaryFooterVisible: boolean;
    showVisibleColumnsSelector: boolean;
    defaultSortColumnAlias: string;
    defaultSortOrder: SortOrder;

    // Foreign key and navigational properties.
    querySourceId?: string;
    widgetSourceId: string;

    widgetSource: IWidgetSource;

    columns: IGridDisplayViewColumn[];

    // Non-database properties.
    queryVariables: string[];
}

export interface IGridDisplayViewColumn {
    id: string;

    dataType: DataType;
    displayMode: DisplayMode;
    extraSettings: any;
    footer: any;
    header: string;
    inputType: InputType;
    queryAlias: string;
    sortOrder: number;
    sortable: boolean;
    visible: boolean;
    variableName: string;
}

export interface IPagedGridSettings {
    skip?: number;
    sortingDirection?: number;
    sortingField?: string;
    take?: number;
}

export interface IQueryPageRequest {
    queryVariables?: IQueryVariables;
    gridSettings?: IPagedGridSettings;
}

export interface IColumnTemplate {
    column: IGridColumn;
    customTemplate: string;
}

export interface IAggregatedValues {
    [key: string]: { value: string, textAlign: string }
}

export type DisplayHandler = (cellValue: any, rowValue?: any) => string;