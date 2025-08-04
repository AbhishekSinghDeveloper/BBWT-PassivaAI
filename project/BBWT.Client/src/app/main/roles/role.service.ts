import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { IRole } from "./role";
import { IHttpResponseHandlerSettings } from "@bbwt/modules/data-service/http-responses-handler";
import { ApiAccessModel } from "./api-access-model";
import { AppStorage } from "@bbwt/utils/app-storage";


@Injectable({
    providedIn: "root"
})
export class RoleService extends PagedCrudService<IRole> {
    readonly url = "api/role";
    readonly entityTitle = "Role";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    get apiAccessModel(): ApiAccessModel {
        return AppStorage.getItem<ApiAccessModel>("api-access-model");
    }

    set apiAccessModel(val: ApiAccessModel) {
        AppStorage.setItem("api-access-model", val);
    }

    get isPermissionBasedModel(): boolean {
        return this.apiAccessModel == ApiAccessModel.PermissionBased;
    }

    initialize(): void {
        this.getApiAccessModel().then(res => {
            this.apiAccessModel = res ? res : ApiAccessModel.PermissionBased;
        }).catch(() => {
            this.apiAccessModel = ApiAccessModel.PermissionBased;
        });
    }

    getCoreRoles(handlerSettings?: IHttpResponseHandlerSettings): Promise<IRole[]> {
        return this.httpGet("core", this.defaultHandler(handlerSettings));
    }

    getProjectRoles(handlerSettings?: IHttpResponseHandlerSettings): Promise<IRole[]> {
        return this.httpGet("project", this.defaultHandler(handlerSettings));
    }

    getApiAccessModel(handlerSettings?: IHttpResponseHandlerSettings): Promise<ApiAccessModel> {
        return this.httpGet("model", this.defaultHandler(handlerSettings));
    }
}