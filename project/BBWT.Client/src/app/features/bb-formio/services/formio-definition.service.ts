import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {CrudService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {FormDefinitionForCreateNewRequest, FormDefinitionParameters, FormIODefinition} from "../dto/form-definition";
import {EntityId} from "@bbwt/interfaces";

@Injectable()
export class FormIODefinitionService extends CrudService<FormIODefinition> {
    public readonly url = "api/formio";
    public readonly entityTitle = "FormIO";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    async sendFormJson(formDefinition: FormDefinitionForCreateNewRequest): Promise<boolean> {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/form-definition`, formDefinition),
            this.handlersFactory.getDefault());
    }

    async getFormJson(id: EntityId, formRevisionId: EntityId, readonly: boolean, body: FormDefinitionParameters): Promise<FormIODefinition> {
        return await this.handleRequest<FormIODefinition>(
            this.http.post<FormIODefinition>(`${this.url}/form-definition/${id}/${formRevisionId}/${readonly}`, body),
            this.handlersFactory.getDefault());
    }
}
