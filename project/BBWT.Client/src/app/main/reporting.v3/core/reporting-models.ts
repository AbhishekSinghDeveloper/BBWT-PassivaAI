import {IVariableRule, IQueryVariables, ExpressionOperator} from "./variables/variable-models";
import {SelectItem} from "primeng/api";


export type WidgetSourceCode = "control-set" | "table" | "chart" | "html";

export function getWidgetSourceCodeLabel(code: WidgetSourceCode): string {
    switch (code) {
        case "control-set":
            return "Control Set";
        case "table":
            return "Table";
        case "chart":
            return "Chart";
        case "html":
            return "HTML";
    }
}

export type DataType = "bool" | "date" | "numeric" | "other" | "string";

export function getDataTypeEnumAsOptions(): SelectItem[] {
    return <SelectItem[]>[
        {label: "Bool", value: "bool"},
        {label: "Date", value: "date"},
        {label: "Numeric", value: "numeric"},
        {label: "Other", value: "other"},
        {label: "String", value: "string"}
    ];
}

export type InputType = "calendar" | "checkbox" | "dropdown" | "multiselect" | "number" | "text" | "textarea";

export interface IVariable {
    id: number;
    name: string;
}

export interface IWidgetSource {
    id: string;
    name?: string;
    code?: string;
    title?: string;
    createdOn: Date;
    isDraft: boolean;
    widgetType: WidgetSourceCode;

    // Foreign keys and navigational properties.
    ownerId?: string;
    ownerName?: string;
    organizationIds: number[];
    releaseWidgetId?: string;
    displayRuleId?: number;
    displayRule?: IVariableRule
}

export interface IWidgetSourcePreload {
    id: string;
    widgetType: WidgetSourceCode;
}

export interface IQuerySource {
    id: string;
    isDraft: boolean;
    queryType: string;
    name: string;
    createdOn: Date;
    filterMode?: QueryFilterMode;

    // Foreign keys and navigational properties.
    ownerId?: string;
    ownerName?: string;
    organizationIds: number[];
    releaseQueryId?: string;
}

export interface ICustomColumnType {
    id: string;
    name: string;
    mask: string;
}

export interface IQuerySchema {
    columns: IQuerySchemaColumn[];
}

export interface IQuerySchemaColumn {
    queryAlias: string;
    dataType: DataType;
}

export interface IQueryColumnAggregation {
    queryAlias: string;
    expressions: string[];
}

export interface IViewMetadata {
    customColumnTypes: ICustomColumnType[];
    columns: IViewMetadataColumn[];
}

export interface IViewMetadataColumn {
    mask?: string;
    minWidth?: number;
    maxWidth?: number;
    queryAlias: string;
    title?: string;
}

export interface ITableSet {
    id: string;
    folderId: string;
    folderSourceCode: string;
}

export interface ITableSetFolder {
    id: string;
    name: string;
    sourceCode: string;
}

export interface ITableSetTable {
    id: string;
    name: string;
    folderId: string;
    tableAlias: string;
    sourceCode: string;
    parentTableId?: string;
    columns: ITableSetColumn[];
    children: ITableSetTable[];
}

export interface ITableSetColumn {
    id: string;
    name: string;
    tableId: string;
    columnAlias: string;
    isPrimaryKey: boolean;
    isForeignKey: boolean;
    table: ITableSetTable;
}

export interface ITableSetFolderInfo {
    id: string;
    sourceCode: string;
    tables: ITableSetTable[];
}

export interface IPagedGridSettings {
    skip?: number;
    take?: number;
    sortingDirection?: number;
    sortingField?: string;
}

export interface IQueryDataRequest {
    tableId: string;
    folderId: string;
    sourceCode: string;
    valueColumnId: string;
    labelColumnId: string;
    filterOperand?: string
    parentTableId?: string;
    filterOperator?: ExpressionOperator
    filterColumnId?: string;
    queryVariables?: IQueryVariables;
}

export interface IQueryPageRequest {
    gridSettings?: IPagedGridSettings;
    queryVariables?: IQueryVariables;
}

export interface PdfConfiguration {
    htmlContent: string;
    cssRules?: string;
    width?: string;
    height?: string;
    margin?: string;
    headerTemplate?: string;
    footerTemplate?: string;
    headerCssRules?: string;
    footerCssRules?: string;
}

export enum QueryFilterMode {
    UserOrganizationFilter = 0,
    UserOrganizationsFilter = 1
}