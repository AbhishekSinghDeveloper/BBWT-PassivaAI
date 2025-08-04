import {Injectable} from "@angular/core";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IQueryCommand, NumberFilter, StringFilter} from "@features/filter";
import {PagedCrudService} from "@features/grid";
import {HttpClient} from "@angular/common/http";
import {MultiUserFormAssociation, NewMultiUserFormAssociation} from "../dto/multi-user-form-associations.dto";


@Injectable()
export class MultiUserFormAssociationsService extends PagedCrudService<MultiUserFormAssociation> {
    public readonly url = "api/multi-user-form-associations";
    public readonly entityTitle = "Multi User Form Associations";
    public multiUserFormDefinitionId: number | null = null;
    public userId: string | null = null;

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    public extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        if (this.multiUserFormDefinitionId) queryCommand.filters.push(new NumberFilter("multiUserFormDefinitionId", this.multiUserFormDefinitionId));
        if (this.userId) queryCommand.filters.push(new StringFilter("userId", this.userId));
    }

    public async addNewMUFAssociation(dto: NewMultiUserFormAssociation) {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/new-mufassoc`, dto),
            this.handlersFactory.getForCreate("New Multi-User Form Association"));
    }

    public async getMUFRenderData(id: string, target: string) {
        return await this.handleRequest<MultiUserFormAssociation>(
            this.http.get<MultiUserFormAssociation>(`${this.url}/render/${id}/${target}`),
            this.handlersFactory.getDefault());
    }
}
