import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { HttpResponsesHandlersFactory, IHttpResponseHandlerSettings } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import {
    IQueryRule,
    IReport,
    ISection,
    IReportView,
    IReportChangeResult,
    IReportLastUpdatedDraftInfo
} from "../reporting-models";
import { IColumnType, IFolder, ITableMetadata } from "@main/dbdoc";
import { IHash } from "@bbwt/interfaces";
import { IRole } from "@main/roles";
import { SelectItem } from "primeng/api";


@Injectable()
export class ReportService extends PagedCrudService<IReport> {
    readonly url = "api/reporting/report";
    readonly entityTitle = "Report";

    static readonly DefaultFolderName = "All Tables";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    addSectionToRow(reportDraftId: string, sectionId: string, rowIndex: number, columnIndex: number)
        : Promise<IHash<{rowIndex: number, columnIndex: number}>> {
        return this.httpPost(`${reportDraftId}/add-section-to-row/${sectionId}/${rowIndex}/${columnIndex}`, null,
            this.handlersFactory.getForUpdate("Report", {
                successMessage: "The section has been moved."
            })
        );
    }

    cancelDraft(reportDraftId: string): Promise<void> {
        return this.httpDelete(`${reportDraftId}/cancel`, this.handlersFactory.getForDelete("Report Draft"));
    }

    createDraftOfNewReport(report: IReport): Promise<IReport> {
        return this.httpPost("create-draft", report,
            this.handlersFactory.getForCreate("New Report Draft", { showSuccessMessage: false })
        );
    }

    createSection(reportId: string, section: ISection): Promise<IReportChangeResult<ISection>> {
        return this.httpPost(`${reportId}/create-section`, section, this.handlersFactory.getForCreate("Section"));
    }

    deleteSection(reportId: string, sectionId: string): Promise<IReportChangeResult> {
        return this.httpDelete(`${reportId}/delete-section/${sectionId}`,
            this.handlersFactory.getForDelete("Section")
        );
    }

    exists(alias: string): Promise<boolean> {
        return this.httpGet(`exists/${alias}`);
    }

    getCurrentUserNewReportDraft(): Promise<IReport> {
        return this.httpGet("get-current-user-report-draft");
    }

    getOrCreateCurrentUserExistingReportDraft(publishedReportId: string): Promise<IReport> {
        return this.httpPost(`current-user-report-draft/${publishedReportId}`, null);
    }

    getQueryRules(): Promise<IQueryRule[]> {
        return this.httpGet("get-query-rules");
    }

    getFolders(): Promise<IFolder[]> {
        return this.httpGet("folders");
    }

    getFolderTableMetadata(folderId: string): Promise<ITableMetadata[]> {
        return this.httpGet(`folder-tables/${folderId}`);
    }

    getFullTableMetadata(tableMetadataId: number): Promise<ITableMetadata> {
        return this.httpGet(`full-table-metadata/${tableMetadataId}`);
    }

    getReportLastUpdatedDraftInfo(reportId: string): Promise<IReportLastUpdatedDraftInfo> {
        return this.httpGet(`${reportId}/get-last-updated-draft-info`);
    }

    getRoleOptions(handlerSettings?: IHttpResponseHandlerSettings): Promise<SelectItem[]> {
        return this.httpGet("role-options", this.defaultHandler(handlerSettings));
    }

    getColumnTypes(handlerSettings?: IHttpResponseHandlerSettings): Promise<IColumnType[]> {
        return this.httpGet("column-types", this.defaultHandler(handlerSettings));
    }

    publishReportDraft(reportDraftId: string): Promise<void> {
        return this.httpPost(`publish-draft/${reportDraftId}`, null,
            this.handlersFactory.getForUpdate("Report", {
                successMessage: "The report draft has been successfully published."
            })
        );
    }

    replaceDraftWithRecent(draftId: string): Promise<IReport> {
        return this.httpPost(`${draftId}/replace-draft-with-recent`, null,
            this.handlersFactory.getForUpdate("Report", {
                successMessage: "The report draft has been successfully replaced with the recent one."
            })
        );
    }

    setSectionRow(reportDraftId: string, sectionId: string, rowIndex: number)
        : Promise<IHash<{rowIndex: number, columnIndex: number}>> {
        return this.httpPost(`${reportDraftId}/set-section-row/${sectionId}/${rowIndex}`, null,
            this.handlersFactory.getForUpdate("Report", {
                successMessage: "The section has been moved."
            })
        );
    }

    useDefaultDbFolder(): Promise<any> {
        return this.httpGet("use-default-db-folder");
    }

    updateSection(reportId: string, sectionId: string, section: ISection): Promise<IReportChangeResult<ISection>> {
        return this.httpPut(`${reportId}/update-section/${sectionId}`, section,
            this.handlersFactory.getDefault()
        );
    }

    getReportView(urlSlug: string): Promise<IReportView> {
        return this.httpGet(`view/${urlSlug}`,
            this.defaultHandler(<IHttpResponseHandlerSettings>{
                errorStatusesMessages: {
                    404: "No report was found. It may have been removed, but the route for this report still exists."
                }
            })
        );
    }
}