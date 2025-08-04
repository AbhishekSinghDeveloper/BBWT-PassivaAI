import { SelectItem } from "primeng/api";

import { IEntity } from "@bbwt/interfaces";
import { IPagedData } from "@features/grid";
import { IColumnType } from "./column-types/column-type-models";


export type AnonymizationRule =
    "date"
    | "elven_name"
    | "email_address"
    | "iban"
    | "random_characters"
    | "random_digits"
    | "roman_name"
    | "string"
    | "uuid";

export function getAnonymizationRuleOptions(): SelectItem[] {
    return <SelectItem[]> [
        { label: "Date", value: "date" },
        { label: "Elven name", value: "elvenName" },
        { label: "Email address", value: "emailAddress" },
        { label: "IBAN", value: "iban" },
        { label: "Random characters", value: "randomCharacters" },
        { label: "Random digits", value: "randomDigits" },
        { label: "Roman name", value: "romanName" },
        { label: "String", value: "string" },
        { label: "UUID", value: "uuid" }
    ];
}

export type ClrTypeGroup = "string" | "numeric" | "date" | "bool" | "other";

export function getClrTypeGroupOptions(): SelectItem[] {
    return <SelectItem[]> [
        { value: "bool", label: "Boolean" },
        { value: "date", label: "Date" },
        { value: "numeric", label: "Numeric" },
        { value: "string", label: "String" }
    ];
}

export type InputFormatType = "phone" | "email" | "url" | "regex";

export type RuleType = "required" | "number_range" | "date_range" | "input_format" | "max_length";

export enum DatabaseType {
    MsSql = 1,
    MySql = 2,
    PostgreSql = 3
}

export function getDatabaseSourceTypeOptions(): SelectItem[] {
    return <SelectItem[]> [
        { label: "MySQL", value: DatabaseType.MySql },
        { label: "Microsoft SQL Server", value: DatabaseType.MsSql },
        { label: "Postgre SQL", value: DatabaseType.PostgreSql }
    ];
}

export enum AnonymizationAction {
    LeaveUnchanged = 0,
    Anonymize = 1,
    Clear = 2
}


export interface IFolder extends IEntity {
    id: string;
    name: string;
    changedOn: Date;
    description: string;
    owners: string[];
    isSourceFolder: boolean;
    protected: boolean;
    databaseSource: IDatabaseSourceDetails;
    tables: ITableMetadata[];
}

export interface IDatabaseSourceDetails extends IEntity {
    id: string;
    name: string;    
    contextId: string;    
    databaseType: DatabaseType
    databaseName: string;
}

export interface ITableMetadata extends IEntity {
    id: number;
    tableId: string;
    description: string;
    anonymization: AnonymizationAction;
    representation: string;
    //TODO: rename to schemaTable
    staticData: ITableStaticData;

    folderId: string;

    columns: IColumnMetadata[];
}

export interface ITableStaticData {
    dbName: string;
    queryName: string;
    schema: string;
    tableName: string;
    isView: boolean;
}

export interface IColumnMetadata extends IEntity {
    id: number;
    columnId: string;
    anonymizationRule: AnonymizationRule;
    description: string;
    title: string;
    //TODO: rename to schemaColumn
    staticData: IColumnStaticData;
    hidden: boolean;

    tableId: number;
    columnTypeId: string;
    columnType: IColumnType;
    validationMetadataId?: number;
    validationMetadata?: IColumnValidationMetadata;
    viewMetadataId?: number;
    viewMetadata?: IColumnViewMetadata;
}

export interface IColumnStaticData {
    allowNull: boolean;
    clrTypeGroup: ClrTypeGroup;
    columnName: string;
    defaultValue: string;
    defaultValueSql: string;
    isForeignKey: boolean;
    isIndex: boolean;
    isPrimaryKey: boolean;
    parentTableName: string;
    propertyName: string;
    queryName: string;
    tableId: string;
    type: string;

    tableReferences: ITableReference[];
}

export interface ITableReference {
    sourceTableId: string;
    sourceColumnId: string;
    targetTableId: string;
    targetColumnId: string;
    isRequired: boolean;
}

export interface IColumnValidationMetadata extends IEntity {
    id: number;
    rules: IValidationRule[];
}

export interface IValidationRule {
    $type: RuleType;
    errorMessage: string;
    isSystem: boolean;
}

export interface IInputFormatValidationRule extends IValidationRule {
    type: InputFormatType;
    format: string;
}

export interface IMaxLengthValidationRule extends IValidationRule {
    maxLength: number;
}

export interface INumberRangeValidationRule extends IValidationRule {
    min?: number;
    max?: number;
}

export interface IDateRangeValidationRule extends IValidationRule {
    min?: Date;
    max?: Date;
}

export interface IColumnViewMetadata extends IEntity {
    id: number;

    gridColumnView?: IGridColumnView;
}

export interface IGridColumnView extends IEntity {
    id: number;
    minWidth?: number;
    maxWidth?: number;
    mask: string;

    columnViewMetadataId: number;
}

export interface IAddDatabaseRequest {
    folderName: string;
    folderDescription: string;
    connectionString: string;
    databaseType: DatabaseType;
}

export interface ICopyTableMetadataToFolderRequest {
    folderIdCopyTo: string;
    copyingTableMetadataId: number;
}

export interface ITableData {
    columns: Array<{ item1: string, item2: string }>;
    data: IPagedData<any>;
}

export interface ISaveTableEntityRequest {
    tableMetadataId: number;
    entity: IEntity;
}

export interface IDeleteTableEntityRequest {
    uniqueTableId: string;
    entityId: string;
}

export interface IColumnMetadataResult {
    validationRules: IValidationRule[];
    gridColumnView: IGridColumnView;
}

export interface ITableMetadataResult {
    [columnName: string]: IColumnMetadataResult;
}

export interface ITableDataViewSettings {
    showTableData: boolean;
    readOnlyTableData: boolean;
}

export interface ISetColumnTypeMetadataForColumnMetadataRequest {
    columnMetadataId: number;
    columnTypeId: string;
}