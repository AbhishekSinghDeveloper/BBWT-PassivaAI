import { Injectable } from "@angular/core";
import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IQueryCommand } from "@features/filter";
import { PagedCrudService } from "@features/grid";
import { HttpClient } from "@angular/common/http";
import { FormRequestDTO, RequestTargets } from "../dto/formRequestDTO";

@Injectable()
export class FormioRequestService extends PagedCrudService<FormRequestDTO> {
    public readonly url = "api/formio-request";
    public readonly entityTitle = "Form requests";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    public extendQueryCommand(queryCommand: IQueryCommand) {
        /*
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberArrayFilter("orgIds", this.userService.currentUser.organizations.map(x => x.id_original)));
        queryCommand.filters.push(new BooleanFilter("isAdmin", this.userService.currentUser.isSuperAdmin || this.userService.currentUser.isSystemAdmin));
        queryCommand.filters.push(new StringFilter("userID", this.userService.currentUser.id));
        */
    }

    async getRequestTargets(): Promise<RequestTargets> {
        return await this.handleRequest<RequestTargets>(
            this.http.get<RequestTargets>(`${this.url}/targets`),
            this.handlersFactory.getDefault());
    }
    async createRequest(request: FormRequestDTO): Promise<boolean> {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/new`, request),
            this.handlersFactory.getForCreate("Form requests"));
    }

    
}
