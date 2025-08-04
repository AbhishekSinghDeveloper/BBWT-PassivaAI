import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {FormRevisionDTO, NewFormRevisionRequest, UpdateFormRevisionRequest} from "../dto/form-revision";
import {IQueryCommand, NumberFilter} from "@features/filter";
import {PagedCrudService} from "@features/grid";

@Injectable()
export class FormIORevisionService extends PagedCrudService<FormRevisionDTO> {
    public readonly url = "api/formio-revision";
    public readonly entityTitle = "Form Revision";
    public formDefinitionId = -1;

    public extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberFilter("formDefinitionId", this.formDefinitionId));
    }

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    public async updateFormRevision(formRevisionId: string, requestData: UpdateFormRevisionRequest): Promise<boolean> {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/${formRevisionId}`, requestData),
            this.handlersFactory.getForUpdate(this.entityTitle));
    }

    public async createFormRevision(requestData: NewFormRevisionRequest): Promise<boolean> {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/new`, requestData),
            this.handlersFactory.getForCreate(this.entityTitle));
    }
}