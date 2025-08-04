import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";

import { DashboardService } from "./dashboard.service";
import { IDashboardView } from "../dashboard-models";


@Injectable()
export class DashboardViewPageResolver {
    constructor(private service: DashboardService) {}

    resolve(route: ActivatedRouteSnapshot): Promise<IDashboardView> {
        return this.service.getViewByCode(route.params["urlSlug"]);
    }
}