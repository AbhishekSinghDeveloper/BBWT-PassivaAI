import { RouterStateSnapshot, ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";

import { IReportView } from "../reporting-models";
import { ReportService } from "./report.service";


@Injectable()
export class ReportViewResolver  {
    constructor(private service: ReportService) {}

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<IReportView> {
        const urlSlug = route.params["urlSlug"];
        if (urlSlug) {
            return this.service.getReportView(urlSlug);
        }
    }
}