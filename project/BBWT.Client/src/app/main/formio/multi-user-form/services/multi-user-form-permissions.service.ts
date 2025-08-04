import {Injectable} from "@angular/core";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {CountableFilterMatchMode, IQueryCommand, NumberFilter} from "@features/filter";
import {PagedCrudService} from "@features/grid";
import {HttpClient} from "@angular/common/http";
import {MultiUserFormPermission, NewMultiUserFormPermission} from "../dto/multi-user-form-permissions.dto";


@Injectable()
export class MultiUserFormPermissionsService extends PagedCrudService<MultiUserFormPermission> {
    public readonly url = "api/multi-user-form-permissions";
    public readonly entityTitle = "Multi User Form Stage's Permissions";
    public mufStageId: number;

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    public extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberFilter("multiUserFormStageId", this.mufStageId, CountableFilterMatchMode.Equals));
    }

    public async addNewPermission(dto: NewMultiUserFormPermission) {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/new-permission`, dto),
            this.handlersFactory.getForCreate("New Multi-User Form Stage Permission"));
    }
}
