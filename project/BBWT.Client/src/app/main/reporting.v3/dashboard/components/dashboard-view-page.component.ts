import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { IDashboardView } from "../dashboard-models";

@Component({
    templateUrl: "./dashboard-view-page.component.html"
})
export class DashboardViewPageComponent {
    _dashboardView: IDashboardView;

    constructor(private activatedRoute: ActivatedRoute) {
        activatedRoute.data.subscribe(data => this._dashboardView = data["dashboardView"]);
    }
}