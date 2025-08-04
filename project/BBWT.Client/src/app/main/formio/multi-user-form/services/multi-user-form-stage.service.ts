import {Injectable} from "@angular/core";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IQueryCommand, NumberFilter} from "@features/filter";
import {PagedCrudService} from "@features/grid";
import {HttpClient} from "@angular/common/http";
import {MUFUserGroupTargets, MultiUserFormStage, MultiUserFormStageUpdate} from "../dto/multi-user-form-stage.dto";


@Injectable()
export class MultiUserFormStageService extends PagedCrudService<MultiUserFormStage> {
    public readonly url = "api/multi-user-form-stage";
    public readonly entityTitle = "Multi User Form Stages";
    public mufDefId: number;

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    public extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberFilter("multiUserFormDefinitionId", this.mufDefId));
    }

    public async getGroupTargets() {
        return await this.handleRequest<MUFUserGroupTargets[]>(
            this.http.get<MUFUserGroupTargets[]>(`${this.url}/group-targets`),
            this.handlersFactory.getDefault());
    }

    public async updateStage(dto: MultiUserFormStageUpdate) {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/update-stage`, dto),
            this.handlersFactory.getForUpdate("Multi-User Form Stage"));
    }
}
