import { SectionService } from "../services/section.service";
import {
    IFilterControl,
    IGridView,
    IGridViewColumn,
    IQueryFilterBinding, IReport,
    ISection
} from "../reporting-models";
import { ReportTransformer } from "../classes/report-transformer";
import { IViewBuilderHandler } from "../interfaces/view-builder-handler";


export class ViewBuilderHandler implements IViewBuilderHandler {
    constructor(private sectionService: SectionService, private report: IReport, private section: ISection) {}


    addFilterControl(filterControl: IFilterControl): Promise<void> {
        return this.sectionService.addFilterControl(this.section.id, filterControl).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.addFilterControl(
                this.section,
                result.requestTargetPart,
                <IQueryFilterBinding> result.additionalChangedParts[0]?.changedPartData);
        });
    }

    deleteFilterControl(filterControl: IFilterControl, deleteLinkedQueryFilters: boolean): Promise<void> {
        return this.sectionService.deleteFilterControl(this.section.id, filterControl.id, deleteLinkedQueryFilters)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.deleteFilterControl(this.section, filterControl, deleteLinkedQueryFilters);
            });
    }

    moveFilterControl(fromIndex: number, toIndex: number): Promise<void> {
        ReportTransformer.moveFilterControl(this.section, fromIndex, toIndex);
        return this.sectionService.updateFilterControl(
            this.section.id,
            this.section.view.filters[toIndex].id,
            this.section.view.filters[toIndex]).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
        });
    }

    moveGridViewColumn(fromIndex: number, toIndex: number): Promise<void> {
        ReportTransformer.moveGridViewColumn(this.section, fromIndex, toIndex);
        return this.sectionService.updateGridViewColumn(
            this.section.id,
            this.section.view.gridView.viewColumns[toIndex].id,
            this.section.view.gridView.viewColumns[toIndex]).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
        });
    }

    toggleAllGridViewColumnsSortable(value: boolean): Promise<void> {
        return this.sectionService.toggleSectionAllGridViewColumnsSortable(this.section.id, value);
    }

    toggleAllGridViewColumnsVisible(value: boolean): Promise<void> {
        return this.sectionService.toggleSectionAllGridViewColumnsVisible(this.section.id, value);
    }

    updateFilterControl(filterControl: IFilterControl): Promise<void> {
        return this.sectionService.updateFilterControl(this.section.id, filterControl.id, filterControl).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);

            result.requestTargetPart.queryFilterBindings = filterControl.queryFilterBindings.filter(x => x.id);
            result.additionalChangedParts.forEach(x => {
                (<IQueryFilterBinding> x.changedPartData).queryFilter.queryFilterBindings = [(<IQueryFilterBinding> x.changedPartData)];
            });

            ReportTransformer.updateFilterControl(
                this.section,
                result.requestTargetPart,
                result.additionalChangedParts.map(x => <IQueryFilterBinding> x.changedPartData));
        });
    }

    updateGridView(gridView: IGridView): Promise<void> {
        return this.sectionService.updateGridView(this.section.id, gridView.id, gridView).then(result => {
            ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
            ReportTransformer.updateGridView(this.section, result.requestTargetPart);
        });
    }

    updateGridViewColumn(gridViewColumn: IGridViewColumn): Promise<void> {
        return this.sectionService.updateGridViewColumn(this.section.id, gridViewColumn.id, gridViewColumn)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.updateGridViewColumn(this.section, result.requestTargetPart);
            });
    }
}