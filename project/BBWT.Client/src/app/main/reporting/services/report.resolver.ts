import { RouterStateSnapshot, ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";

import { IReport } from "../reporting-models";
import { ReportService } from "./report.service";


@Injectable()
export class ReportResolver  {
    constructor(private service: ReportService) {}

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<IReport> {
        const reportId = route.params["reportId"];
        return reportId
            ? this.service.getOrCreateCurrentUserExistingReportDraft(reportId)
            : this.service.getCurrentUserNewReportDraft();
    }
}