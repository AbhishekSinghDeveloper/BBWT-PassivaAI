import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IQueryCommand } from "@features/filter";
import { IPagedData } from "@features/grid";
import { IOrder } from "@demo/northwind";

@Injectable({
    providedIn: "root"
})
export class SecurityAccessibleService extends BaseDataService {
    readonly url = "api/demo/security-test";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getDataForAccessibleToAnyAuthenticated(queryCommand: IQueryCommand): Promise<IPagedData<IOrder>> {
        return this.handle(
            this.http.get<IPagedData<IOrder>>(`${this.url}/accessible/authenticated`,
                { params: this.constructHttpParams(queryCommand) }),
            this.handlersFactory.getForReadByFilter()
        );
    }

    getByGroupNameForAccessibleToGroup(groupName: string): Promise<IOrder> {
        return this.httpGet(`accessible/group/${groupName}`, this.handlersFactory.getForReadById("Car"));
    }
}