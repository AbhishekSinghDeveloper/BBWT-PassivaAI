import {Component} from "@angular/core";
import {ActivatedRoute, Router} from "@angular/router";

@Component({
    templateUrl: "./dashboard-editor-page.component.html"
})
export class DashboardEditorPageComponent {
    _dashboardId: string;

    constructor(
        private router: Router,
        private activatedRoute: ActivatedRoute) {

        activatedRoute.params.subscribe(params => {
            this._dashboardId = params["dashboardId"];
        });

    }
}