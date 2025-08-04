import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { IReportView } from "../reporting-models";


@Component({
    templateUrl: "./report-view-page.component.html"
})
export class ReportViewPageComponent {
    reportView: IReportView;

    constructor(private activatedRoute: ActivatedRoute) {
        activatedRoute.data.subscribe(data => this.reportView = data["reportView"]);
    }
}