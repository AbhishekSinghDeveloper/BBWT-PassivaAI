import { SelectItem } from "primeng/api";

import { IPermission, IRole } from "@main/roles";
import { IEntity } from "@bbwt/interfaces";
import { SortOrder } from "@features/grid";
import { IColumnMetadata, IColumnStaticData, IColumnType } from "@main/dbdoc";
import { FilterInputType, FilterType } from "@features/filter";
import { IUser } from "@main/users";


function fillQueryFiltersTreeSet(set: IQueryFilterSet, allSets: IQueryFilterSet[]): void {
    set.childSets = allSets.filter(x => x.parentId === set.id);
    set.childSets.findIndex(childSet => fillQueryFiltersTreeSet(childSet, allSets));
}

export function buildQueryFiltersTree(query: IQuery): void {
    query.rootFilterSet = query.queryFilterSets.find(x => x.parentQueryId);
    fillQueryFiltersTreeSet(query.rootFilterSet, query.queryFilterSets);
}

export function buildReportSectionsPositionStructure(sections: ISection[]): Array<Array<ISection>> {
    const result = [];

    let sectionRowIndex = -1;
    let sectionsRow: ISection[] = [];
    sections.sort((a, b) => a.rowIndex < b.rowIndex ? -1 : a.rowIndex > b.rowIndex ? 1 : 0)
        .forEach(sectionItem => {
            if (sectionRowIndex !== sectionItem.rowIndex) {
                if (sectionsRow.length) {
                    sectionsRow.sort((a, b) =>
                        a.columnIndex < b.columnIndex ? -1 : a.columnIndex > b.columnIndex ? 1 : 0);
                    result.push(sectionsRow);
                }

                sectionRowIndex = sectionItem.rowIndex;
                sectionsRow = [];
            }

            sectionsRow.push(sectionItem);
        });

    sectionsRow.sort((a, b) =>
        a.columnIndex < b.columnIndex ? -1 : a.columnIndex > b.columnIndex ? 1 : 0);

    result.push(sectionsRow);

    return result;
}

export function getFullColumnName(columnStaticData: IColumnStaticData): string {
    return `${columnStaticData.parentTableName}.${columnStaticData.columnName}`;
}

export type ExpandBehaviour = "noContainer" | "static" | "initiallyExpanded" | "initiallyCollapsed";
export function getExpandBehaviourEnumAsOptions(): SelectItem[] {
    return <SelectItem[]>[
        { label: "No container", value: "noContainer" },
        { label: "Static", value: "static" },
        { label: "Initially expanded", value: "initiallyExpanded" },
        { label: "Initially collapsed", value: "initiallyCollapsed" }
    ];
}

export type SectionDataViewType = "dataGrid" | "noView";
export function getSectionDataViewTypeEnumAsOptions(): SelectItem[] {
    return <SelectItem[]>[
        { label: "Data Grid", value: "dataGrid" },
        { label: "No View", value: "noView" }
    ];
}

export type QueryConditionalOperator = "and" | "or";
export function getQueryConditionalOperatorEnumAsOptions(): SelectItem[] {
    return <SelectItem[]>[
        { label: "And", value: "and" },
        { label: "Or", value: "or" }
    ];
}

export function trueFalseOptions(): SelectItem[] {
    return <SelectItem[]>[
        { label: "True", value: true },
        { label: "False", value: false }
    ];
}

export type QueryRuleCode = "equals" | "notEquals" | "more" | "moreOrEqual" |
    "less" | "lessOrEqual" | "between" | "contains" | "notContains" | "startsWith" | "endsWith";
export function getQueryRuleCodeLabel(code: QueryRuleCode): string {
    switch (code) {
        case "equals": return "Equal";
        case "notEquals": return "Not equal";
        case "more": return "More";
        case "moreOrEqual": return "More or equal";
        case "less": return "Less";
        case "lessOrEqual": return "Less or equal";
        case "between": return "Between";
        case "contains": return "Contains";
        case "notContains": return "Not contains";
        case "startsWith": return "Starts with";
        case "endsWith": return "Ends with";
    }

    return "";
}

export function getQueryRuleOperator(code: QueryRuleCode): string {
    switch (code) {
        case "equals": return "=";
        case "notEquals": return "!=";
        case "more": return ">";
        case "moreOrEqual": return ">=";
        case "less": return "<";
        case "lessOrEqual": return "<=";
    }

    return "";
}

export type QueryRuleDataType = "string" | "numeric" | "boolean" | "datetime";

export type QueryFilterBindingType = "filterControl" | "masterDetailGrid";

export type SqlFilterVariableType = "tableColumn" | "filterControl" | "unknown";

export type ReportChangeType = "created" | "modified" | "deleted";

export type MasterSectionEmitEventType = "rowSelected"; // | ..


export interface IReport extends IEntity {
    id: string;
    name: string;
    showTitle: boolean;
    urlSlug: string;

    /// Determines a type of users who can access the report.
    /// "authenticated" - for any authenticated user
    /// "<empty value>" - access is determined by roles/permissions sets
    access: string;

    createdOn: Date;
    updatedOn: Date;
    isDraft: boolean;
    createdBy: string;
    updatedBy: string;
    updatedByUser: IUser;
    publishedReport: IReport;
    publishedReportId: string;

    roles: IRole[];
    permissions: IPermission[];
    sections: ISection[];
}

export interface IReportLastUpdatedDraftInfo {
    draftId: string;
    owner: string;
    updatedOn: Date;
}

export interface ISection extends IEntity {
    id: string;
    title: string;
    showTitle: boolean;
    dataViewType: SectionDataViewType;
    description: string;
    expandBehaviour: ExpandBehaviour;
    autoCollapse: boolean;
    visible: boolean;
    rowIndex: number;
    columnIndex: number;

    publishedSectionId: string;
    reportId: string;
    report: IReport;
    namedQueryId: string;
    namedQuery: INamedQuery;
    reusedSectionId: string;
    reusedSection: ISection;
    queryId: string;
    query: IQuery;

    view: IView;
}

export interface INamedQuery extends IEntity {
    id: string;
    name: string;
    createdOn: Date;
    updatedOn: Date;
    isDraft: boolean;

    createdBy: string;
    updatedBy: string;
    queryId: string;
    query: IQuery;
}

export interface IQuery extends IEntity {
    id: string;
    dbDocFolderId: string;
    forEndUserOnly: boolean;

    rootFilterSet: IQueryFilterSet;

    queryTables: IQueryTable[];
    queryFilterSets: IQueryFilterSet[];
    queryTableJoins: IQueryTableJoin[];
}

export interface IQueryFilterSet extends IEntity {
    id: string;
    conditionalOperator: QueryConditionalOperator;

    queryId: string;
    query: IQuery;
    parentId?: string;
    parent?: IQueryFilterSet;
    parentQueryId?: string;
    parentQuery?: IQuery;

    childSets: IQueryFilterSet[];
    queryFilters: IQueryFilter[];
}

export interface ISqlFilterCodeInsert {
    variableName: string;
    variableType: SqlFilterVariableType;
    position: number;
}

export interface IQueryFilter extends IEntity {
    id: string;

    value: any;
    value2: any;

    customSqlCodeTemplate: string;
    customSqlCodeInserts: ISqlFilterCodeInsert[];

    queryFilterSetId: string;
    queryFilterSet: IQueryFilterSet;
    queryTableColumnId?: string;
    queryTableColumn: IQueryTableColumn;
    queryRuleId?: string;
    queryRule: IQueryRule;

    queryFilterBindings: IQueryFilterBinding[];
}

export interface IQueryRule extends IEntity {
    id: string;
    name: string;
    code: QueryRuleCode;
    mySqlCodeTemplate: string;
    msSqlCodeTemplate: string;

    ruleTypes: IQueryRuleType[];
}

export interface IQueryRuleType extends IEntity {
    id: string;
    type: QueryRuleDataType;

    queryRuleId: string;
    queryRule: IQueryRule;
}

export interface IQueryTable extends IEntity {
    id: string;
    alias: string;
    sourceTableId: string;
    sourceCode: string;
    selfJoinDbDocColumnId: string;
    onlyForJoin: boolean;

    queryId: string;
    query: IQuery;

    columns: IQueryTableColumn[];
}

export interface IQueryTableColumn extends IEntity {
    id: string;
    sourceColumnId: string;
    onlyForJoin: boolean;

    queryTableId: string;
    queryTable: IQueryTable;
}

export interface IQueryTableJoin extends IEntity {
    id: string;
    fromDbDocColumnId: string;
    fromDbDocTableId: string;
    toDbDocColumnId: string;
    toDbDocTableId: string;

    fromQueryTable: IQueryTable;
    fromQueryTableId: string;
    fromQueryTableColumn: IQueryTableColumn;
    fromQueryTableColumnId: string;
    toQueryTable: IQueryTable;
    toQueryTableId: string;
    toQueryTableColumn: IQueryTableColumn;
    toQueryTableColumnId: string;
    queryId: string;
    query: IQuery;
}

export interface IView extends IEntity {
    id: string;
    sectionId: string;
    section: ISection;

    gridView: IGridView;
    filters: IFilterControl[];
}

export interface IGridView extends IEntity {
    id: string;
    defaultSortOrder: SortOrder;
    showVisibleColumnsSelector: boolean;
    summaryFooterVisible: boolean;

    viewId: string;
    view: IView;
    defaultSortColumnId: string;
    defaultSortColumn: IQueryTableColumn;

    viewColumns: IGridViewColumn[];
}

export interface IGridViewColumn extends IEntity {
    id: string;
    sortOrder: number;
    inheritHeader: boolean;
    header: string;
    sortable: boolean;
    visible: boolean;
    extraSettings: any;
    footer: any;

    gridViewId: string;
    gridView: IGridView;
    queryTableColumnId: string;
    queryTableColumn: IQueryTableColumn;
    customColumnTypeId: string;
    customColumnType: IColumnType;
}

export interface IFilterControl extends IEntity {
    id: string;
    name: string;
    sortOrder: number;
    hintText: string;
    inputType: FilterInputType;
    dataType?: FilterType;
    autoSubmitInput: boolean;
    userCanChangeOperator: boolean;
    extraSettings: IFilterControlExtraSettings;

    viewId: string;
    view: IView;
    masterControlId: string;
    masterControl: IFilterControl;

    queryFilterBindings: IQueryFilterBinding[];
}

export interface IFilterControlExtraSettings {
    sourceDbDocTableId: string;
    labelDbDocColumnId: string;
    valueDbDocColumnId: string;
    enableSourceLookupSearch: boolean;
    sourceUniqueValues: boolean;
}

export interface IQueryFilterBinding extends IEntity {
    id: string;
    bindingType: QueryFilterBindingType;

    queryFilterId?: string;
    queryFilter?: IQueryFilter;
    filterControlId?: string;
    filterControl?: IFilterControl;
    masterDetailSectionId?: string;
    masterDetailSection?: ISection;
    masterDetailQueryTableColumnId?: string;
    masterDetailQueryTableColumn?: IQueryTableColumn;
}

export interface IReportView {
    name: string;
    showTitle: boolean;
    sections: Array<ISection>;
}

export interface ISectionDisplayView {
    title: string;
    showTitle: boolean;
    description: string;
    dataViewType: SectionDataViewType;
    expandBehaviour: ExpandBehaviour;
    autoCollapse: boolean;
    defaultSortOrder: SortOrder;
    showVisibleColumnsSelector: boolean;
    summaryFooterVisible: boolean;
    defaultSortColumn: string;
    columns: ISectionViewColumn[];
    filters: ISectionViewFilter[];

    /** A list of event types that a section being in master-section role, should emit to client-sections */
    masterSectionEmittedEvents: MasterSectionEmitEventType[];

    /** A list of bindings to master-section(s) used by a client-section to handle emitted event */
    masterSectionBindings: IMasterSectionBinding[];
}

export interface ISectionViewColumn {
    tableAlias: string;
    sortOrder: number;
    inheritHeader: boolean;
    header: string;
    sortable: boolean;
    visible: boolean;
    extraSettings: any;
    footer: any;
    dbDocColumnMetadata: IColumnMetadata;
    customColumnType: IColumnType;
}

export interface ISectionViewFilter {
    filterControlId: string;
    sortOrder: number;
    hintText: string;
    inputType: FilterInputType;
    dataType?: FilterType;
    autoSubmitInput: boolean;
    userCanChangeOperator: boolean;
    extraSettings: IFilterControlExtraSettings;
    dbDocColumnId: string;
    queryFilterId: string;
    name: string;
    queryRuleCode: QueryRuleCode;
}

export interface IMasterSectionEvent {
    eventType: MasterSectionEmitEventType;
    masterSectionId: string;
    data: any;
}

export interface IMasterSectionBinding {
    masterSectionId: string;
    eventType: MasterSectionEmitEventType;
    /** A parameter that the master section passes to client section
     * (e.g. in case of row selecting event, it passes column name applied as query filter to the dependent grid). */
    filterId: string;
    columnId: string;
}

export interface IReportChangeResult<T = any> {
    reportUpdatedOn: Date;
    requestTargetPart?: T;
    additionalChangedParts: IReportAdditionalChangedPart[];
}

export interface IReportAdditionalChangedPart {
    changedPartData: any;
    changedPartId: any;
    changedPartName: string;
    changedPartType: ReportChangeType;
}

export interface ISaveMasterDetailBindingCommandEventData {
    queryTableColumnId?: string;
    queryFilterSet?: IQueryFilterSet;
    bindingId?: string;
    masterSectionId?: string;
    masterSectionTableColumnId?: string;
    queryFilter?: IQueryFilter;
}