import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {IPagedData} from "@features/grid";
import {QueryCommand} from "@features/filter";
import {IDashboard, IDashboardBuild, IDashboardView} from "@main/reporting.v3/dashboard/dashboard-models";
import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";

@Injectable({
    providedIn: "root"
})
export class DashboardService extends BaseDataService {
    readonly url = "api/reporting3/dashboard";
    readonly entityTitle = "Dashboard";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getBuild(dashboardId: string): Promise<IDashboardBuild> {
        return this.httpGet(`${dashboardId}/build`);
    }

    getView(dashboardId: string): Promise<IDashboardView> {
        return this.httpGet(`${dashboardId}/view`);
    }

    getViewByCode(dashboardCode: string): Promise<IDashboardView> {
        return this.httpGet(`${dashboardCode}/view-by-code`);
    }

    getPage(queryCommand: QueryCommand): Promise<IPagedData<IDashboard>> {
        return this.handle<IPagedData<IDashboard>>(
            this.http.get<IPagedData<IDashboard>>(`${this.url}/page`,
                {params: this.constructHttpParams(queryCommand)})
        )
    }

    create(dashboard: IDashboardBuild): Promise<IDashboardBuild> {
        return this.httpPost(null, dashboard, this.handlersFactory.getForCreate(this.entityTitle));
    }

    update(dashboardId: string, dashboard: IDashboardBuild): Promise<IDashboardBuild> {
        return this.httpPut(`${dashboardId}`, dashboard, this.handlersFactory.getForUpdate(this.entityTitle));
    }

    delete(dashboardId: string): Promise<void> {
        return this.httpDelete(`${dashboardId}`, this.handlersFactory.getForDelete(this.entityTitle));
    }

    publishDashboard(dashboardId: string, organizationIds: number[]): Promise<void> {
        return this.httpPut(`${dashboardId}/publish`, organizationIds,
            this.handlersFactory.getForUpdate(this.entityTitle));
    }

    changeOwner(dashboardId: string, ownerId: string): Promise<void> {
        return this.handle(this.http.put<void>(`${this.url}/${dashboardId}/change-owner`,
                null, {params: this.constructHttpParams({ownerId: ownerId})}),
            this.handlersFactory.getForUpdate(this.entityTitle));
    }
}