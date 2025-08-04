import { SectionService } from "../services/section.service";
import {
    IGridViewColumn,
    IQueryFilter,
    IQueryFilterBinding,
    IQueryFilterSet, IQueryTable, IQueryTableColumn, IQueryTableJoin, IReport,
    ISection
} from "../reporting-models";
import { ReportTransformer } from "./report-transformer";
import { IQueryBuilderHandler } from "../interfaces/query-builder-handler";
import { IQueryableTableSource } from "../model/queryable-table-sources-models";


export class QueryBuilderHandler implements IQueryBuilderHandler {
    constructor(private sectionService: SectionService, private report: IReport, private section: ISection) {}


    addDuplicateQueryTable(queryTableJoin: IQueryTableJoin): Promise<void> {
        return this.sectionService.addDuplicateQueryTable(this.section.id, queryTableJoin).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);

            ReportTransformer.addQueryTable(
                this.section,
                result.requestTargetPart.fromQueryTable,
                result.additionalChangedParts
                    .filter(x => x.changedPartName === "GridViewColumn" &&
                        result.requestTargetPart.fromQueryTable.columns
                            .some(y => y.id === (<IGridViewColumn> x.changedPartData).queryTableColumnId))
                    .map(x => <IGridViewColumn> x.changedPartData),
                []);

            ReportTransformer.addQueryTable(
                this.section,
                result.requestTargetPart.toQueryTable,
                result.additionalChangedParts
                    .filter(x => x.changedPartName === "GridViewColumn" &&
                        result.requestTargetPart.toQueryTable.columns
                            .some(y => y.id === (<IGridViewColumn> x.changedPartData).queryTableColumnId))
                    .map(x => <IGridViewColumn> x.changedPartData),
                [result.requestTargetPart]);
        });
    }

    addQueryFilter(parentQueryFilterSet: IQueryFilterSet, queryFilter: IQueryFilter): Promise<void> {
        return this.sectionService.addQueryFilter(this.section.id, queryFilter).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            result.requestTargetPart.queryFilterSet = null;
            ReportTransformer.addQueryFilter(parentQueryFilterSet, result.requestTargetPart);
        });
    }

    addQueryFilterSet(parentQueryFilterSet: IQueryFilterSet): Promise<void> {
        return this.sectionService.addQueryFilterSet(this.section.id, parentQueryFilterSet.id).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.addQueryFilterSet(this.section, parentQueryFilterSet, result.requestTargetPart);
        });
    }

    addQueryTable(tableMetadataId: number): Promise<IQueryTable> {
        return this.sectionService.addQueryTable(this.section.id, tableMetadataId).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.addQueryTable(
                this.section,
                result.requestTargetPart,
                result.additionalChangedParts
                    .filter(x => x.changedPartName === "GridViewColumn")
                    .map(x => <IGridViewColumn> x.changedPartData),
                result.additionalChangedParts
                    .filter(x => x.changedPartName === "QueryTableJoin")
                    .map(x => <IQueryTableJoin> x.changedPartData));

            return result.requestTargetPart;
        });
    }

    addQueryTablesFromSource(sources: IQueryableTableSource[]): Promise<void> {
        return this.sectionService.addQueryTablesFromSource(this.section.id, sources).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);

            result.requestTargetPart.forEach(table => {
                ReportTransformer.addQueryTable(
                    this.section,
                    table,
                    result.additionalChangedParts.map(x => <IGridViewColumn>x.changedPartData));
            });
        });
    }

    addQueryTableColumn(columnMetadataId: number, parentQueryTableId?: number): Promise<void> {
        return this.sectionService.addQueryTableColumn(this.section.id, columnMetadataId, parentQueryTableId).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.addQueryTableColumn(
                this.section,
                result.requestTargetPart,
                result.additionalChangedParts
                    .find(x => x.changedPartName === "GridViewColumn" && x.changedPartType === "created")
                    .changedPartData,
                result.additionalChangedParts
                    .filter(x => x.changedPartName === "GridViewColumn" && x.changedPartType === "modified")
                    .map(x => <IGridViewColumn>x.changedPartData),
                result.additionalChangedParts
                    .filter(x => x.changedPartName === "QueryTableJoin")
                    .map(x => <IQueryTableJoin> x.changedPartData)
            );
        });
    }

    addQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<IQueryTableJoin> {
        return this.sectionService.addQueryTableJoin(this.section.id, queryTableJoin).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.addQueryTableJoin(this.section, result.requestTargetPart);

            return result.requestTargetPart;
        })
    }

    bindFilterControlToQueryFilter(filterControlId: string, queryFilter: IQueryFilter): Promise<void> {
        return this.sectionService.bindFilterControlToQueryFilter(this.section.id, filterControlId, queryFilter.id)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.bindFilterControlToQueryFilter(
                    this.section,
                    this.section.view.filters.find(x => x.id == filterControlId),
                    result.requestTargetPart);
            });
    }

    deleteQueryFilter(queryFilter: IQueryFilter): Promise<void> {
        return this.sectionService.deleteQueryFilter(this.section.id, queryFilter.id).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.deleteQueryFilter(this.section, queryFilter);
        });
    }

    deleteQueryFilterBinding(queryFilterBinding: IQueryFilterBinding): Promise<void> {
        return this.sectionService.deleteQueryFilterBinding(this.section.id, queryFilterBinding.id).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.deleteQueryFilterBinding(this.section, queryFilterBinding);
        });
    }

    deleteQueryFilterSet(queryFilterSet: IQueryFilterSet): Promise<void> {
        return this.sectionService.deleteQueryFilterSet(this.section.id, queryFilterSet.id).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.deleteQueryFilterSet(this.section, queryFilterSet);
        });
    }

    deleteQueryTable(queryTable: IQueryTable): Promise<void> {
        return this.sectionService.deleteQueryTable(this.section.id, queryTable.id).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.deleteQueryTable(this.section, queryTable);
        });
    }

    deleteQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<void> {
        return this.sectionService.deleteQueryTableJoin(this.section.id, queryTableJoin.id).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.deleteQueryTableJoin(this.section, queryTableJoin);
        });
    }

    deleteQueryTableColumn(queryTableColumn: IQueryTableColumn): Promise<void> {
        return this.sectionService.deleteQueryTableColumn(this.section.id, queryTableColumn.id).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.deleteQueryTableColumn(this.section, queryTableColumn);
        });
    }

    updateMasterDetailFilterBinding(queryFilterBinding: IQueryFilterBinding): Promise<void> {
        return this.sectionService.updateMasterDetailQueryFilterBinding(this.section.id, queryFilterBinding)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.updateMasterDetailFilterBinding(
                    this.section,
                    <IQueryFilter> result.additionalChangedParts[0].changedPartData,
                    result.requestTargetPart);
            });
    }

    updateQueryFilter(queryFilter: IQueryFilter): Promise<void> {
        return (queryFilter.queryTableColumnId ?
            this.sectionService.updateQueryFilter(this.section.id, queryFilter.id, queryFilter) :
            this.sectionService.updateSqlFilter(this.section.id, queryFilter.id, queryFilter))
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.updateQueryFilter(this.section, result.requestTargetPart);
            });
    }

    updateQueryFilterSet(queryFilterSet: IQueryFilterSet): Promise<void> {
        return this.sectionService.updateQueryFilterSet(this.section.id, queryFilterSet.id, queryFilterSet)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.updateQueryFilterSet(this.section, result.requestTargetPart);
            });
    }

    updateQueryTableJoin(queryTableJoin: IQueryTableJoin): Promise<IQueryTableJoin> {
        return this.sectionService.updateQueryTableJoin(this.section.id, queryTableJoin.id, queryTableJoin)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.updateQueryTableJoin(this.section, result.requestTargetPart);

                return result.requestTargetPart;
            });
    }
}