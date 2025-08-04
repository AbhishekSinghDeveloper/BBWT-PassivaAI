import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IGroup } from "@main/users/group";
import { PagedCrudService } from "@features/grid";

@Injectable({
    providedIn: "root"
})
export class GroupsService extends PagedCrudService<IGroup> {
    readonly url = "api/demo/group";
    readonly entityTitle = "Group";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}