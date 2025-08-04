import {Injectable} from "@angular/core";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IQueryCommand} from "@features/filter";
import {PagedCrudService} from "@features/grid";
import {HttpClient} from "@angular/common/http";
import {FormIODefinition} from "@features/bb-formio";
import {MultiUserFormDef, NewMultiUserFormDefinition} from "../dto/multi-user-form.dto";
import {MUFUserGroupTargets} from "../dto/multi-user-form-stage.dto";


@Injectable()
export class MultiUserFormService extends PagedCrudService<MultiUserFormDef> {
    public readonly url = "api/multi-user-form";
    public readonly entityTitle = "Multi User Form";

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

    public async getFormDefinitions() {
        return await this.handleRequest<FormIODefinition[]>(
            this.http.get<FormIODefinition[]>(`${this.url}/form-definitions`),
            this.handlersFactory.getDefault());
    }

    public async getUserTargets() {
        return await this.handleRequest<MUFUserGroupTargets[]>(
            this.http.get<MUFUserGroupTargets[]>(`${this.url}/user-targets`),
            this.handlersFactory.getDefault());
    }

    public async getInstanceTargets(id: string) {
        return await this.handleRequest<MUFUserGroupTargets[]>(
            this.http.get<MUFUserGroupTargets[]>(`${this.url}/instance-targets/${id}`),
            this.handlersFactory.getDefault());
    }

    public async addNewMUF(dto: NewMultiUserFormDefinition) {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/new-muf`, dto),
            this.handlersFactory.getForCreate("New Multi-User Form"));
    }

    public async isMUFReady(id: string) {
        return await this.handleRequest<boolean>(
            this.http.get<boolean>(`${this.url}/muf-ready/${id}`),
            this.handlersFactory.getDefault());
    }
}
